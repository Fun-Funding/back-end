using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Domain.Constrain;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly ClaimsPrincipal _claimsPrincipal;
        public readonly IAzureService _azureService;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public UserService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IAzureService azureService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _mapper = mapper;
            _azureService = azureService;
        }
        public async Task<ResultDTO<PaginatedResponse<UserInfoResponse>>> GetUsers(ListRequest request)
        {
            try
            {
                Expression<Func<User, bool>> filter = null;
                Expression<Func<User, object>> orderBy = u => u.CreatedDate;
                const string excludeRoleName = "Administrator";

                // Get users with Administrator role
                var excludeRole = await _roleManager.FindByNameAsync(excludeRoleName);
                var userIdsToExclude = new List<string>();

                if (excludeRole != null)
                {
                    var usersInExcludeRole = await _userManager.GetUsersInRoleAsync(excludeRoleName);
                    userIdsToExclude = usersInExcludeRole.Select(u => u.Id.ToString()).ToList();
                }

                // Combine search filter with role exclusion
                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    string searchLower = request.SearchValue.ToLower();
                    if (userIdsToExclude.Any())
                    {
                        filter = u =>
                            !userIdsToExclude.Contains(u.Id.ToString()) &&
                            ((u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                             (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                             (u.UserName != null && u.UserName.ToLower().Contains(searchLower)));
                    }
                    else
                    {
                        filter = u =>
                            (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                            (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                            (u.UserName != null && u.UserName.ToLower().Contains(searchLower));
                    }
                }
                else
                {
                    if (userIdsToExclude.Any())
                    {
                        filter = u => !userIdsToExclude.Contains(u.Id.ToString());
                    }
                }

                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "fullname":
                            orderBy = u => u.FullName;
                            break;
                        case "email":
                            orderBy = u => u.Email;
                            break;
                        default:
                            break;
                    }
                }

                var list = await _unitOfWork.UserRepository.GetAllDeletedAsync(
                       filter: filter,
                       orderBy: orderBy,
                       isAscending: request.IsAscending.Value,
                       pageIndex: request.PageIndex,
                       pageSize: request.PageSize,
                       includeProperties: "Wallet");

                if (list != null && list.Count() >= 0)
                {
                    var totalItems = _unitOfWork.UserRepository.GetAllDeletedNoPagination(filter).Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                    IEnumerable<UserInfoResponse> users = _mapper.Map<IEnumerable<UserInfoResponse>>(list);

                    PaginatedResponse<UserInfoResponse> response = new PaginatedResponse<UserInfoResponse>
                    {
                        PageSize = request.PageSize.Value,
                        PageIndex = request.PageIndex.Value,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = users
                    };

                    return ResultDTO<PaginatedResponse<UserInfoResponse>>.Success(response, "User found!");
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User Not Found.");
                }
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<User>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var usersInRole = new List<User>();

                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    var users = _userManager.Users.ToList();
                    foreach (var user in users)
                    {
                        if (await _userManager.IsInRoleAsync(user, roleName))
                        {
                            usersInRole.Add(user);
                        }
                    }
                }

                return usersInRole;
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<ResultDTO<UserInfoResponse>> GetUserInfo()
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "User not authenticated.");
                }
                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userEmail = userEmailClaims.Value;
                var user = await _unitOfWork.UserRepository.GetQueryable()
                                .AsNoTracking()
                                .Include(u => u.File)
                                .Include(u => u.Wallet)
                                .FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userInfoResponse = _mapper.Map<UserInfoResponse>(user);

                return ResultDTO<UserInfoResponse>.Success(userInfoResponse, "User found!");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<UserInfoResponse>> GetUserInfoById(Guid id)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(u => u.File)
                    .Include(u => u.Wallet)
                        .ThenInclude(u => u.Transactions)
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (user is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userResponse = _mapper.Map<UserInfoResponse>(user);
                return ResultDTO<UserInfoResponse>.Success(userResponse, "User found!");

            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<string>> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "User not authenticated.");
                }
                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userEmail = userEmailClaims.Value;
                User user = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (user == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                if (userUpdateRequest.DayOfBirth != null)
                {
                    var tenYearsAgo = DateTime.Today.AddYears(-10);
                    if (userUpdateRequest.DayOfBirth > tenYearsAgo)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "User must be at least 10 years old.");
                    }
                }
                _mapper.Map(userUpdateRequest, user);
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.CommitAsync();

                return ResultDTO<string>.Success("", "Update Successfully");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<UserInfoResponse>> ChangeUserStatus(Guid id)
        {
            try
            {
                var parseUser = await _unitOfWork.UserRepository.GetQueryable()
                                .Include(u => u.Wallet)
                                .FirstOrDefaultAsync(u => u.Id == id);
                if (parseUser is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                UserInfoResponse userResponse = new UserInfoResponse();
                if (parseUser.UserStatus == UserStatus.Active)
                {
                    parseUser.UserStatus = UserStatus.Inactive;
                    await _unitOfWork.CommitAsync();
                    parseUser = await _userManager.FindByIdAsync(id.ToString());
                    userResponse = _mapper.Map<UserInfoResponse>(parseUser);
                    return ResultDTO<UserInfoResponse>.Success(userResponse, "Status changed successfully to Inactive");
                }
                parseUser.UserStatus = UserStatus.Active;
                await _unitOfWork.CommitAsync();
                parseUser = await _userManager.FindByIdAsync(id.ToString());
                userResponse = _mapper.Map<UserInfoResponse>(parseUser);
                return ResultDTO<UserInfoResponse>.Success(userResponse, "Status changed successfully to Active");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<string>> ChangeUserPassword(UserChangePasswordRequest userChangePasswordRequest)
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "User not authenticated.");
                }

                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }

                var userEmail = userEmailClaims.Value;
                User user = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (user == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }

                if (userChangePasswordRequest.NewPassword != userChangePasswordRequest.ConfirmPassword)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "New password and confirmation password do not match.");
                }

                var result = await _userManager.ChangePasswordAsync(user, userChangePasswordRequest.OldPassword, userChangePasswordRequest.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, $"Password change failed: {errors}");
                }

                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.CommitAsync();

                return ResultDTO<string>.Success("", "Password updated successfully");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<string>> UploadUserAvatar(UserFileRequest userFileRequest)
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "User not authenticated.");
                }

                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userEmail = userEmailClaims.Value;
                var user = await _unitOfWork.UserRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(u => u.File)
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }

                if (user.File == null)
                {
                    UserFile file = new UserFile();
                    if (userFileRequest.URL.Length > 0)
                    {
                        var result = _azureService.UploadUrlSingleFiles(userFileRequest.URL);
                        if (result == null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "File upload failed. Please try again.");
                        }
                        else
                        {
                            file.Name = userFileRequest.Name;
                            file.URL = result.Result;
                            file.Filetype = 0;
                            file.UserId = user.Id;
                            file.CreatedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Uploaded file is empty.");
                    }
                    user.File = file;
                    _unitOfWork.UserFileRepository.Add(file);
                    _unitOfWork.UserRepository.Update(user);
                    await _unitOfWork.CommitAsync();
                    return ResultDTO<string>.Success("", "Upload avatar successfully");
                }
                else
                {
                    UserFile file = user.File;
                    if (userFileRequest.URL.Length > 0)
                    {
                        var result = _azureService.UploadUrlSingleFiles(userFileRequest.URL);
                        if (result == null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "File upload failed. Please try again.");
                        }
                        else
                        {
                            file.Name = userFileRequest.Name;
                            file.URL = result.Result;
                            file.Filetype = 0;
                            file.UserId = user.Id;
                            file.CreatedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Uploaded file is empty.");
                    }
                    _unitOfWork.UserFileRepository.Update(file);
                    await _unitOfWork.CommitAsync();
                    return ResultDTO<string>.Success("", "Upload avatar successfully");
                }
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<string>> CheckUserPassword(string password)
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "User not authenticated.");
                }

                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }

                var userEmail = userEmailClaims.Value;
                User user = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (user == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }

                var result = await _userManager.CheckPasswordAsync(user, password);

                if (!result)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Password validation failed.");
                }

                return ResultDTO<string>.Success("", "Password validated successfully");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<string>> CheckUserRole(User user)
        {
            // Get the currently logged-in user
            if (user != null)
            {
                return ResultDTO<string>.Fail("No User Found");
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Role.Admin))
            {
                // User is an Admin
                return ResultDTO<string>.Success(Role.Admin, "logged as Admin");
            }
            else if (roles.Contains(Role.Backer))
            {
                // User is a normal User
                return ResultDTO<string>.Success(Role.Backer, "logged as Backer");
            }

            return ResultDTO<string>.Success(Role.GameOwner, "logged as Owner");
        }

        public async Task<ResultDTO<List<TopBackerResponse>>> GetTop4Backer()
        {
            try
            {
                var allBackers = await _unitOfWork.PackageBackerRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(pb => pb.User)
                    .ToListAsync();

                var nonAdminBackers = new List<PackageBacker>();

                foreach (var backer in allBackers)
                {
                    var roles = await _userManager.GetRolesAsync(backer.User);
                    if (!roles.Contains(Role.Admin))
                    {
                        nonAdminBackers.Add(backer);
                    }
                }

                var topBackers = nonAdminBackers
                    .GroupBy(b => b.UserId)
                    .Select(group => new
                    {
                        UserId = group.Key,
                        TotalDonation = group.Sum(b => b.DonateAmount)
                    })
                    .OrderByDescending(x => x.TotalDonation)
                    .Take(4)
                    .ToList();

                var result = topBackers.Select(tb => new TopBackerResponse
                {
                    Id = tb.UserId,
                    AvatarURL = _unitOfWork.UserRepository.GetById(tb.UserId).File?.URL ?? "",
                    UserName = _unitOfWork.UserRepository.GetById(tb.UserId).UserName,
                    TotalDonation = tb.TotalDonation
                }).ToList();

                return ResultDTO<List<TopBackerResponse>>.Success(result, "Backer Found!");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<decimal>> CountPlatformUsers()
        {
            try
            {
                var count = await _unitOfWork.UserRepository.GetQueryable().CountAsync();
                return ResultDTO<decimal>.Success(count, "Count number of users");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<string>> GetUserRole(Guid id)
        {
            var parseUser = await _unitOfWork.UserRepository.GetQueryable().AsNoTracking()
                                .Include(u => u.Wallet)
                                .FirstOrDefaultAsync(u => u.Id == id);
            if (parseUser is null)
            {
                throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
            }
            var roles = await _userManager.GetRolesAsync(parseUser);
            if (roles.Contains(Role.Admin))
            {
                return ResultDTO<string>.Success(Role.Admin, "logged as Admin");
            }
            else if (roles.Contains(Role.Backer))
            {
                return ResultDTO<string>.Success(Role.Backer, "logged as Backer");
            }

            return ResultDTO<string>.Success(Role.GameOwner, "logged as Owner");
        }

        public async Task<ResultDTO<UserInfoResponse>> CreateUser(UserCreateRequest request)
        {
            try
            {
                if (request.Password == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.InternalServerError, "Password field is required!");
                }
                if (request.Email == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.InternalServerError, "Email field is required!");
                }
                if (request.UserName == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.InternalServerError, "Username field is required!");
                }
                if (request.FullName == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.InternalServerError, "Full name field is required!");
                }

                var existingUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == request.Email);
                if (existingUser != null)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "User already exists with this email.");
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = request.FullName,
                    UserName = request.UserName,
                    PhoneNumber = request.PhoneNumber ?? null,
                    Address = request.Address ?? null,
                    DayOfBirth = request.DayOfBirth ?? null,
                    Email = request.Email,
                    CreatedDate = DateTime.Now,
                    Gender = request.Gender ?? null,
                    UserStatus = request.UserStatus,
                    NormalizedEmail = request.Email.ToUpper(),
                    TwoFactorEnabled = false,
                    EmailConfirmed = true,
                };

                if (request.File != null)
                {
                    if (request.File.URL != null && request.File.URL.Length > 0)
                    {
                        UserFile file = new UserFile();
                        var res = _azureService.UploadUrlSingleFiles(request.File.URL);
                        if (res == null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "File upload failed. Please try again.");
                        }
                        else
                        {
                            file.Name = request.File.Name;
                            file.URL = res.Result;
                            file.Filetype = 0;
                            file.UserId = newUser.Id;
                            file.CreatedDate = DateTime.Now;
                        }
                        newUser.File = file;
                    }
                }

                var result = await _userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new ExceptionError((int)HttpStatusCode.InternalServerError, errors);
                }

                if (request.Role == 1)
                {
                    await _userManager.AddToRoleAsync(newUser, Role.GameOwner);
                }
                else
                {
                    await _userManager.AddToRoleAsync(newUser, Role.Backer);
                }

                var bankAccount = new BankAccount
                {
                    Id = Guid.NewGuid(),
                    BankCode = string.Empty,
                    BankNumber = string.Empty,
                    CreatedDate = DateTime.Now,
                };

                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    Balance = 0,
                    Backer = newUser,
                    BankAccountId = bankAccount.Id,
                    CreatedDate = bankAccount.CreatedDate,
                };

                await _unitOfWork.BankAccountRepository.AddAsync(bankAccount);
                await _unitOfWork.WalletRepository.AddAsync(wallet);
                await _unitOfWork.CommitAsync();

                var user = await _unitOfWork.UserRepository.GetQueryable()
                   .AsNoTracking()
                   .Include(u => u.File)
                   .Include(u => u.Wallet)
                       .ThenInclude(u => u.Transactions)
                   .FirstOrDefaultAsync(u => u.Id == newUser.Id);
                if (user is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userResponse = _mapper.Map<UserInfoResponse>(user);
                return ResultDTO<UserInfoResponse>.Success(userResponse, "Add User Successfully");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
