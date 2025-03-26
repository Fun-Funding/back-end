using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CategoryDTO;
using Fun_Funding.Application.ViewModel.FundingFileDTO;
using Fun_Funding.Application.ViewModel.FundingProjectDTO;
using Fun_Funding.Application.ViewModel.PackageDTO;
using Fun_Funding.Domain.Constrain;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class FundingProjectManagementService : IFundingProjectService
    {
        public IUnitOfWork _unitOfWork;
        private readonly ClaimsPrincipal _claimsPrincipal;
        public IMapper _mapper;
        public IAzureService _azureService;
        private int maxDays = 60;
        private int minDays = 1;
        public IUserService _userService;
        public string defaultImage = "https://funfundingmediafiles.blob.core.windows.net/fundingprojectfiles/sampleThumb_a1abaa10-9b59-465b-a31b-218031942496.jfif";
        private INotificationService _notificationService;
        public FundingProjectManagementService(IUnitOfWork unitOfWork, IMapper mapper, IAzureService azureService, IHttpContextAccessor httpContextAccessor, IUserService userService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _azureService = azureService;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _userService = userService;
            _notificationService = notificationService;
        }
        public async Task<ResultDTO<FundingProjectResponse>> CreateFundingProject(FundingProjectAddRequest projectRequest)
        {
            try
            {
                //find authorize user
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
                User owner = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (owner == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Owner not found");
                }

                //map project
                FundingProject project = _mapper.Map<FundingProject>(projectRequest);
                project.User = owner;
                //validate category
                project.Categories.Clear();
                foreach (CategoryProjectRequest cate in projectRequest.Categories)
                {
                    Category category = _unitOfWork.CategoryRepository.GetById(cate.Id);
                    if (category == null)
                    {
                        throw new ExceptionError((int)HttpStatusCode.NotFound, "Category not found");
                    }
                    project.Categories.Add(category);
                }
                //validate package amount
                var packageNames = new HashSet<string>();
                foreach (PackageAddRequest pack in projectRequest.Packages)
                {
                    // Check for duplicate package names
                    if (!packageNames.Add(pack.Name)) // Add will return false if the name already exists
                    {
                        throw new ExceptionError((int)HttpStatusCode.NotFound, $"Duplicate package name found: {pack.Name}");
                    }
                    if (pack.RequiredAmount < 5000)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Price for package must be at least 5000");
                    }
                    if (pack.RewardItems.Count < 1)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Each package must have at least 1 item");
                    }
                    if (pack.LimitQuantity < 1)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Each package must limit at least 1 quantity");
                    }
                }
                //validate bank
                if (projectRequest.BankAccount is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project must config its bank account for payment");
                }

                //create project wallet
                Wallet projectWallet = new Wallet
                {
                    Balance = 0
                };
                BankAccount bank = new BankAccount
                {
                    Wallet = projectWallet,
                    BankCode = projectRequest.BankAccount.BankCode,
                    BankNumber = projectRequest.BankAccount.BankNumber,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false

                };
                project.Wallet = projectWallet;
                //validate startDate endDate info
                if (project.StartDate < DateTime.Now)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Start date cannot be before today");
                }
                if (project.EndDate <= project.StartDate)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "End date must be greater that start date");
                }
                if ((project.EndDate - project.StartDate).TotalDays < minDays || (project.EndDate - project.StartDate).TotalDays > maxDays)
                {
                    return ResultDTO<FundingProjectResponse>.Fail("Funding campaign length must be at least 1 day and maximum 60 days");
                }
                project.CreatedDate = DateTime.Now;
                project.Status = ProjectStatus.Pending;
                //free package
                Package freePack = new Package
                {
                    Name = "Non-package support",
                    Description = "We offer the \"Non-Package Support\" option, allowing you to contribute any amount you choose. This flexible choice doesn’t include rewards " +
                    "but greatly supports our project. Any amount given is greatly appreciated!",
                    PackageTypes = PackageType.Free,
                    CreatedDate = DateTime.Now,
                    RequiredAmount = 0
                };

                //add files 
                List<FundingFile> files = new List<FundingFile>();
                if (projectRequest.FundingFiles.Count == 0 || projectRequest.FundingFiles == null)
                {
                    FundingFile media = new FundingFile
                    {
                        Name = "Default Image",
                        URL = defaultImage,
                        Filetype = 0
                    };
                    files.Add(media);
                }
                foreach (FundingFileRequest req in projectRequest.FundingFiles)
                {
                    if (req.URL.Length > 0)
                    {
                        var res = _azureService.UploadUrlSingleFiles(req.URL);
                        if (res == null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "Fail to upload file");
                        }
                        FundingFile media = new FundingFile
                        {
                            Name = req.Name,
                            URL = res.Result,
                            Filetype = req.Filetype
                        };
                        files.Add(media);
                    }
                }
                //add iamge into item 
                foreach (var package in project.Packages)
                {
                    var pack = projectRequest.Packages
                            .FirstOrDefault(p => p.Name == package.Name);
                    if (pack?.ImageFile != null)
                    {
                        var packageImage = _azureService.UploadUrlSingleFiles(pack.ImageFile);
                        package.Url = packageImage.Result;
                    }

                    foreach (var rewardItem in package.RewardItems)
                    {
                        // Find the corresponding reward item in the request to get its ImageFile
                        var rewardRequest = projectRequest.Packages
                            .FirstOrDefault(p => p.Name == package.Name)?
                            .RewardItems.FirstOrDefault(r => r.Name == rewardItem.Name);

                        // If ImageFile is present, upload it and set the ImageUrl
                        if (rewardRequest?.ImageFile != null)
                        {
                            var uploadResult = _azureService.UploadUrlSingleFiles(rewardRequest.ImageFile);
                            rewardItem.ImageUrl = uploadResult.Result; // Append the uploaded URL to the mapped reward item
                        }
                    }
                    package.CreatedDate = DateTime.Now;
                    package.PackageTypes = PackageType.FixedPackage;
                }
                project.Status = ProjectStatus.Pending;
                project.SourceFiles = files;
                project.Packages.Add(freePack);
                _unitOfWork.FundingProjectRepository.Add(project);
                _unitOfWork.BankAccountRepository.Add(bank);
                _unitOfWork.Commit();
                return GetProjectById(project.Id).Result;
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public async Task<ResultDTO<FundingProjectResponse>> GetProjectById(Guid id)
        {
            try
            {
                var project = _unitOfWork.FundingProjectRepository.GetQueryable()
                    .Include(p => p.Packages).ThenInclude(pack => pack.RewardItems)
                    .Include(p => p.SourceFiles.Where(sf => sf.IsDeleted == false))
                    .Include(p => p.Wallet).ThenInclude(w => w.BankAccount)
                    .Include(p => p.User)
                    .Include(p => p.Categories)
                    .AsSplitQuery()
                    .FirstOrDefault(p => p.Id == id);
                var transaction = _unitOfWork.TransactionRepository.GetQueryable()
                    .FirstOrDefault(t => t.WalletId == project.Wallet.Id && t.TransactionType == TransactionTypes.WithdrawFundingMilestone);
                var cancelTransaction = _unitOfWork.TransactionRepository.GetQueryable()
                    .FirstOrDefault(t => t.WalletId == project.Wallet.Id && t.TransactionType == TransactionTypes.WithdrawCancel);
                if (project is null)
                {
                    return ResultDTO<FundingProjectResponse>.Fail("Project not found", 404);
                }
                FundingProjectResponse result = _mapper.Map<FundingProjectResponse>(project);
                if (transaction != null)
                {
                    result.HasBeenWithdrawed = true;
                }
                else
                {
                    result.HasBeenWithdrawed = false;
                }
                if (cancelTransaction != null)
                {
                    result.HasBeenWithdrawed = false;
                }
                return ResultDTO<FundingProjectResponse>.Success(result);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw ex;
            }
        }

        public async Task<ResultDTO<FundingProjectResponse>> UpdateFundingProject(FundingProjectUpdateRequest projectRequest)
        {
            try
            {
                var existedProject = _unitOfWork.FundingProjectRepository.GetQueryable()
                .Include(p => p.SourceFiles)
                .Include(p => p.Packages).ThenInclude(pack => pack.RewardItems)
                .FirstOrDefault(o => o.Id == projectRequest.Id);
                // check status
                if (existedProject == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Project Not Found.");
                }
                if (existedProject.Status != ProjectStatus.Processing)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project is not in processing status");
                }
                //update regulations for funding goals and end date

                //validate bank and package amount
                foreach (PackageUpdateRequest pack in projectRequest.Packages)
                {
                    if (pack.RequiredAmount < 5000)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Price for package must be at least 5000");
                    }
                    if (pack.RewardItems.Count < 1)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Each package must have at least 1 item");

                    }
                    if (pack.LimitQuantity < 1)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Each package must limit at least 1 quantity");
                    }
                }
                if (projectRequest.BankAccount is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project must config its bank account for payment");
                }

                existedProject.Name = projectRequest.Name;
                existedProject.Description = projectRequest.Description;
                existedProject.Introduction = projectRequest.Introduction;
                BankAccount existedBank = _unitOfWork.BankAccountRepository.GetQueryable()
                    .FirstOrDefault(b => b.Id == projectRequest.BankAccount.Id);
                if (existedBank != null)
                {
                    existedBank.BankNumber = projectRequest.BankAccount.BankNumber;
                    existedBank.BankCode = projectRequest.BankAccount.BankCode;

                }
                _unitOfWork.BankAccountRepository.Update(existedBank);


                //update if have new files 
                if (projectRequest.FundingFiles?.Count > 0)
                {
                    List<FundingFile> files = new List<FundingFile>();

                    foreach (FundingFileUpdateRequest req in projectRequest.FundingFiles)
                    {
                        if (req.UrlFile.Length > 0)
                        {
                            var res = _azureService.UploadUrlSingleFiles(req.UrlFile);
                            if (res == null)
                            {
                                return ResultDTO<FundingProjectResponse>.Fail("Fail to upload file");
                            }
                            FundingFile media = new FundingFile
                            {
                                Name = req.Name,
                                URL = res.Result,
                                Filetype = req.Filetype,
                                IsDeleted = false
                            };
                            files.Add(media);
                        }
                    }
                    //update existing files
                    if (projectRequest.ExistedFile?.Count > 0)
                    {
                        foreach (FundingFileResponse fp in projectRequest.ExistedFile)
                        {
                            FundingFile updatedFile = _unitOfWork.SourceFileRepository.GetQueryable().FirstOrDefault(f => f.Id == fp.Id);
                            updatedFile.Filetype = fp.Filetype;
                            updatedFile.IsDeleted = fp.IsDeleted;
                            updatedFile.URL = fp.URL;
                            _unitOfWork.SourceFileRepository.Update(updatedFile);
                        }
                    }
                    // Add each file from 'files' list to the 'SourceFiles' ICollection
                    foreach (var file in files)
                    {
                        existedProject.SourceFiles.Add(file);
                    }
                }
                List<Package> packageList = new List<Package>();
                //add image into item 
                foreach (var packageRequest in projectRequest.Packages)
                {

                    var existedPack = existedProject.Packages.FirstOrDefault(p => p.Id == packageRequest.Id);
                    if (existedPack != null)
                    {
                        existedPack.Name = packageRequest.Name;
                        existedPack.RequiredAmount = packageRequest.RequiredAmount;
                        existedPack.LimitQuantity = packageRequest.LimitQuantity;
                        existedPack.PackageTypes = PackageType.FixedPackage;
                        //change image of package
                        if (packageRequest.UpdatedImage != null)
                        {
                            var imageUploadResult = _azureService.UploadUrlSingleFiles(packageRequest.UpdatedImage);
                            existedPack.Url = imageUploadResult.Result;
                        }
                        //Handle change image of existing item
                        foreach (var rewardItemRequest in packageRequest.RewardItems)
                        {
                            var existedRewardItem = _unitOfWork.RewardItemRepository.GetQueryable().FirstOrDefault(r => r.Id == rewardItemRequest.Id);
                            if (existedRewardItem != null)
                            {
                                // Update existing reward item
                                existedRewardItem.Name = rewardItemRequest.Name;
                                existedRewardItem.Description = rewardItemRequest.Description;
                                existedRewardItem.Quantity = rewardItemRequest.Quantity;

                                // Handle image upload for reward item
                                if (rewardItemRequest.ImageFile != null && rewardItemRequest.ImageFile is IFormFile)
                                {
                                    var imageUploadResult = _azureService.UploadUrlSingleFiles(rewardItemRequest.ImageFile);
                                    existedRewardItem.ImageUrl = imageUploadResult.Result;
                                }
                            }
                            else
                            {
                                // Handle adding new reward items if necessary
                                RewardItem newRewardItem = new RewardItem
                                {
                                    Name = rewardItemRequest.Name,
                                    Description = rewardItemRequest.Description,
                                    // Handle image upload for new reward item
                                };
                                if (rewardItemRequest.ImageFile != null && rewardItemRequest.ImageFile is IFormFile)
                                {
                                    var imageUploadResult = _azureService.UploadUrlSingleFiles(rewardItemRequest.ImageFile);
                                    newRewardItem.ImageUrl = imageUploadResult.Result;
                                }
                                existedPack.RewardItems.Add(newRewardItem);
                            }
                        }
                    }
                    else
                    {
                        Package newPackage = new Package
                        {
                            Name = packageRequest.Name,
                            RequiredAmount = packageRequest.RequiredAmount,
                            LimitQuantity = packageRequest.LimitQuantity,
                            PackageTypes = PackageType.FixedPackage,
                            RewardItems = new List<RewardItem>()
                        };
                        //change image of package
                        if (packageRequest.UpdatedImage != null)
                        {
                            var imageUploadResult = _azureService.UploadUrlSingleFiles(packageRequest.UpdatedImage);
                            newPackage.Url = imageUploadResult.Result;
                        }
                        // Add reward items to the new package
                        foreach (var rewardItemRequest in packageRequest.RewardItems)
                        {
                            RewardItem newRewardItem = new RewardItem
                            {
                                Name = rewardItemRequest.Name,
                                Description = rewardItemRequest.Description,
                                Quantity = rewardItemRequest.Quantity,
                            };

                            // Handle image upload for new reward item
                            if (rewardItemRequest.ImageFile != null && rewardItemRequest.ImageFile is IFormFile)
                            {
                                var imageUploadResult = _azureService.UploadUrlSingleFiles(rewardItemRequest.ImageFile);
                                newRewardItem.ImageUrl = imageUploadResult.Result;
                            }

                            newPackage.RewardItems.Add(newRewardItem);
                        }
                        // Add the new package to the project's packages
                        existedProject.Packages.Add(newPackage);
                    }
                }

                _unitOfWork.FundingProjectRepository.Update(existedProject);

                _unitOfWork.Commit();
                FundingProjectResponse result = _mapper.Map<FundingProjectResponse>(existedProject);
                return ResultDTO<FundingProjectResponse>.Success(result);
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
        public async Task<ResultDTO<PaginatedResponse<FundingProjectResponse>>> GetFundingProjects(
            ListRequest request, List<Guid>? categoryIds, List<ProjectStatus>? statusList, decimal? fromTarget, decimal? toTarget)
        {
            try
            {
                // Define filters list
                var filters = new List<Func<IQueryable<FundingProject>, IQueryable<FundingProject>>>();

                // Search filter
                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    string searchLower = request.SearchValue.ToLower();
                    filters.Add(query => query.Where(u => u.Name != null && u.Name.ToLower().Contains(searchLower)));
                }

                // Category filter
                if (categoryIds != null && categoryIds.Any())
                {
                    filters.Add(query => query.Where(c => c.Categories.Any(category => categoryIds.Contains(category.Id))));
                }

                // Date range filters
                if (request.From != null)
                {
                    filters.Add(query => query.Where(c => c.StartDate >= (DateTime)request.From));
                }
                if (request.To != null)
                {
                    filters.Add(query => query.Where(c => c.EndDate <= (DateTime)request.To));
                }

                // Status filter
                if (statusList != null && statusList.Any())
                {
                    filters.Add(query => query.Where(c => statusList.Contains(c.Status)));
                }

                // Target range filters
                if (fromTarget.HasValue)
                {
                    filters.Add(query => query.Where(c => c.Target >= fromTarget.Value));
                }

                if (toTarget.HasValue)
                {
                    filters.Add(query => query.Where(c => c.Target <= toTarget.Value));
                }

                // Determine ordering
                Expression<Func<FundingProject, object>> orderBy = request.OrderBy?.ToLower() switch
                {
                    "balance" => u => u.Balance,
                    "target" => u => u.Target,
                    _ => u => u.CreatedDate
                };

                // Call the repository method
                var list = await _unitOfWork.FundingProjectRepository.GetAllCombinedFilterAsync(
                    filters: filters,
                    orderBy: orderBy,
                    isAscending: request.IsAscending ?? false,
                    pageIndex: request.PageIndex ?? 1,
                    pageSize: request.PageSize ?? 10,
                    includeProperties: "Categories,Packages,SourceFiles,Packages.RewardItems,Wallet,User");

                // Map the results
                var totalItems = _unitOfWork.FundingProjectRepository.GetAllCombinedFilterAsync(filters: filters)
                    .Result.ToList().Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / (request.PageSize ?? 10));
                var projects = _mapper.Map<IEnumerable<FundingProjectResponse>>(list);

                // Prepare the paginated response
                var response = new PaginatedResponse<FundingProjectResponse>
                {
                    PageSize = request.PageSize ?? 10,
                    PageIndex = request.PageIndex ?? 1,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = projects
                };

                return ResultDTO<PaginatedResponse<FundingProjectResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public async Task<ResultDTO<FundingProjectResponse>> UpdateFundingProjectStatus(Guid id, ProjectStatus status, string? note)
        {
            try
            {
                var project = await _unitOfWork.FundingProjectRepository.GetQueryable()
                    .Include(p => p.SourceFiles)
                    .Include(p => p.Packages)
                    .Include(p => p.User)
                    .Include(p => p.Wallet)
                    .FirstOrDefaultAsync(p => p.Id == id);

                //pending status change list
                List<ProjectStatus> pendingChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Approved,
                        ProjectStatus.Rejected,
                        ProjectStatus.Deleted
                    };

                //approved status change list
                List<ProjectStatus> approvedChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Processing,
                        ProjectStatus.Deleted
                    };

                //rejected status change list
                List<ProjectStatus> rejectedChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Deleted
                    };

                //successful status change list
                List<ProjectStatus> successfulChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Withdrawed
                    };


                //processing status change list
                List<ProjectStatus> processingChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Successful,
                        ProjectStatus.Failed
                    };

                //failed status change list
                List<ProjectStatus> failedChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Deleted,
                        ProjectStatus.Refunded
                    };

                bool isChanged = false;

                if (project != null)
                {
                    //change status from pending
                    if (project.Status == ProjectStatus.Pending && pendingChangelist.Contains(status))
                    {
                        if (status == ProjectStatus.Approved)
                        {
                            await CreateInitFundingMilestone(project.Id);
                        }
                        project.Status = status;
                        project.Note = note;
                        isChanged = true;
                    }
                    //change status from approved
                    else if (project.Status == ProjectStatus.Approved && approvedChangelist.Contains(status))
                    {
                        project.Status = status;
                        isChanged = true;
                    }
                    //change status from rejected
                    else if (project.Status == ProjectStatus.Rejected && rejectedChangelist.Contains(status))
                    {
                        project.Status = status;
                        isChanged = true;
                    }
                    //change status from processing
                    else if (project.Status == ProjectStatus.Processing && processingChangelist.Contains(status))
                    {
                        project.Status = status;
                        isChanged = true;
                    }
                    //change status from successful
                    else if (project.Status == ProjectStatus.Successful && successfulChangelist.Contains(status))
                    {
                        project.Status = status;
                        isChanged = true;
                    }
                    //change status from failed
                    else if (project.Status == ProjectStatus.Failed && failedChangelist.Contains(status))
                    {
                        project.Status = status;
                        isChanged = true;
                    }

                    if (isChanged)
                    {
                        _unitOfWork.FundingProjectRepository.Update(project);
                        await _unitOfWork.CommitAsync();

                        var response = _mapper.Map<FundingProject, FundingProjectResponse>(project);

                        // NOTIFICATION
                        // 1. get recipientsIds
                        List<Guid> recipientsId = new List<Guid>();
                        recipientsId.Add(project.UserId);
                        //recipientsId.Add(Guid.Parse("f766c910-4f6a-421e-a1a3-61534e6005c3"));
                        // 2. initiate new Notification object
                        var notification = new Notification
                        {
                            Id = new Guid(),
                            Date = DateTime.Now,
                            Message = $"has updated status project <b>{project.Name}</b>",
                            NotificationType = NotificationType.FundingProjectStatus,
                            Actor = new { Id = new Guid(), UserName = "Admin", Avatar = "" },
                            ObjectId = project.Id,
                        };

                        await _notificationService.SendNotification(notification, recipientsId);

                        return ResultDTO<FundingProjectResponse>.Success(response);
                    }
                    else throw new ExceptionHandler.ExceptionError(
                        (int)HttpStatusCode.BadRequest,
                        $"Funding Project with status {project.Status} cannot be changed to {status}.");
                }
                else
                {
                    throw new ExceptionHandler.ExceptionError((int)HttpStatusCode.NotFound, "Funding Project Not Found.");
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

        public async Task CreateInitFundingMilestone(Guid projectId)
        {
            try
            {
                // Fetch all funding milestones from the database
                var fundingMilestones = await _unitOfWork.MilestoneRepository.GetQueryable()
                    .Where(m => m.MilestoneType == MilestoneType.Funding)
                    .ToListAsync();

                if (fundingMilestones == null || !fundingMilestones.Any())
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "No funding milestones found.");
                }
                var project = _unitOfWork.FundingProjectRepository.GetQueryable().
                    Select(p => new
                    {
                        p.Id,
                        p.CreatedDate,
                        p.EndDate
                    }).FirstOrDefault(p => p.Id == projectId);

                // Create ProjectMilestone entries for each funding milestone
                var projectMilestones = fundingMilestones.Select(fundingMilestone => new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    MilestoneId = fundingMilestone.Id,
                    FundingProjectId = projectId,
                    Title = fundingMilestone.MilestoneName,
                    Introduction = fundingMilestone.Description,
                    Status = ProjectMilestoneStatus.Processing,
                    TotalAmount = null, // Initial total amount
                    CreatedDate = project.CreatedDate,
                    EndDate = project.EndDate,
                    IssueLog = null
                }).ToList();

                // Add project milestones to the database in a batch
                await _unitOfWork.ProjectMilestoneRepository.AddRangeAsync(projectMilestones);

                // Commit the transaction
                await _unitOfWork.CommitAsync();
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

        public async Task<ResultDTO<bool>> CheckProjectOwner(Guid projectId)
        {
            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                User user = _mapper.Map<User>(authorUser._data);
                //user = _unitOfWork.UserFileRepository.GetQueryable()
                if (authorUser is null)
                    return ResultDTO<bool>.Fail("can not found user");
                var project = _unitOfWork.FundingProjectRepository.GetQueryable()
                    .Include(p => p.User)
                    .Include(p => p.Packages)
                    .ThenInclude(pk => pk.PackageUsers)

                    .FirstOrDefault(p => p.Id == projectId);
                if (_claimsPrincipal.IsInRole(Role.GameOwner))
                {
                    if (project.UserId != user.Id)
                    {
                        return ResultDTO<bool>.Success(false, "not owner");
                    }
                    else
                    {
                        return ResultDTO<bool>.Success(true, "owner");
                    }
                }
                bool userExistsInPackageUsers = project.Packages
               .Any(pkg => pkg.PackageUsers.Any(pu => pu.UserId == user.Id));

                if (!userExistsInPackageUsers)
                {
                    return ResultDTO<bool>.Success(false, "not backer of this project");
                }
                return ResultDTO<bool>.Success(true, "backer of this project");


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<ResultDTO<List<FundingProjectResponse>>> GetTop3MostFundedOngoingFundingProject()
        {
            try
            {
                var projects = await _unitOfWork.FundingProjectRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(p => p.SourceFiles)
                    .Include(p => p.Categories)
                    .Include(p => p.User)
                    .Where(p => p.Status == ProjectStatus.Processing || p.Status == ProjectStatus.FundedSuccessful)
                    .ToListAsync();

                var topProjects = projects
                    .OrderByDescending(p => p.Status == ProjectStatus.Processing)
                    .ThenByDescending(p => p.Balance)
                    .Take(3)
                    .ToList();

                List<FundingProjectResponse> result = _mapper.Map<List<FundingProjectResponse>>(topProjects);

                return ResultDTO<List<FundingProjectResponse>>.Success(result, "Funding Project Found!");
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
        public async Task<ResultDTO<PaginatedResponse<FundingProjectResponse>>> GetGameOwnerFundingProjects(ListRequest request, string? categoryName, ProjectStatus? status, decimal? fromTarget, decimal? toTarget)
        {
            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                if (authorUser is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Game Owner Not Found.");
                }
                User user = _mapper.Map<User>(authorUser._data);

                Expression<Func<FundingProject, bool>> filter = u => u.UserId == user.Id;
                Expression<Func<FundingProject, object>> orderBy = u => u.CreatedDate;

                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    string searchLower = request.SearchValue.ToLower();
                    filter = u =>
                        (u.Name != null && u.Name.ToLower().Contains(searchLower));
                }
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "balance":
                            orderBy = u => u.Balance;
                            break;
                        case "target":
                            orderBy = u => u.Target;
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(categoryName))
                {
                    filter = c => c.Categories.Any(category => category.Name.ToLower().Contains(categoryName.ToLower()));
                }
                if (request.From != null)
                {
                    filter = c => c.StartDate >= (DateTime)request.From;
                }
                if (request.To != null)
                {
                    filter = c => c.EndDate <= (DateTime)request.To;
                }
                if (status != null)
                {
                    filter = c => c.Status.Equals(status);
                }
                if (fromTarget != null)
                {
                    filter = c => c.Target >= fromTarget;
                }
                if (toTarget != null)
                {
                    filter = c => c.Target <= toTarget;
                }
                var list = await _unitOfWork.FundingProjectRepository.GetAllAsync(
                       filter: filter,
                       orderBy: orderBy,
                       isAscending: request.IsAscending.Value,
                       pageIndex: request.PageIndex,
                       pageSize: request.PageSize,
                       includeProperties: "Categories,Packages,SourceFiles,Packages.RewardItems,Wallet,User");
                if (list != null && list.Count() >= 0)
                {
                    var totalItems = _unitOfWork.FundingProjectRepository.GetAll(filter).Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                    IEnumerable<FundingProjectResponse> projects = _mapper.Map<IEnumerable<FundingProjectResponse>>(list);

                    PaginatedResponse<FundingProjectResponse> response = new PaginatedResponse<FundingProjectResponse>
                    {
                        PageSize = request.PageSize.Value,
                        PageIndex = request.PageIndex.Value,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = projects
                    };

                    return ResultDTO<PaginatedResponse<FundingProjectResponse>>.Success(response);
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Project Not Found.");
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<PaginatedResponse<FundingProjectResponse>>> GetBackerDonatedProjects(ListRequest request, string? categoryName, ProjectStatus? status, decimal? fromTarget, decimal? toTarget)
        {
            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                if (authorUser is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Backer Not Found.");
                }
                User user = _mapper.Map<User>(authorUser._data);

                List<PackageBacker> pbList = _unitOfWork.PackageBackerRepository.GetQueryable().Where(pb => pb.UserId.Equals(user.Id)).ToList();

                // Extract Package IDs
                var packageIds = pbList.Select(pb => pb.PackageId).ToList();
                //Add filter to FundingProject via Package IDs
                Expression<Func<FundingProject, bool>> filter = u => u.Packages.Any(p => packageIds.Contains(p.Id));
                Expression<Func<FundingProject, object>> orderBy = u => u.CreatedDate;

                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    string searchLower = request.SearchValue.ToLower();
                    filter = u =>
                        (u.Name != null && u.Name.ToLower().Contains(searchLower));
                }
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "balance":
                            orderBy = u => u.Balance;
                            break;
                        case "target":
                            orderBy = u => u.Target;
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(categoryName))
                {
                    filter = c => c.Categories.Any(category => category.Name.ToLower().Contains(categoryName.ToLower()));
                }
                if (request.From != null)
                {
                    filter = c => c.StartDate >= (DateTime)request.From;
                }
                if (request.To != null)
                {
                    filter = c => c.EndDate <= (DateTime)request.To;
                }
                if (status != null)
                {
                    filter = c => c.Status.Equals(status);
                }
                if (fromTarget != null)
                {
                    filter = c => c.Target >= fromTarget;
                }
                if (toTarget != null)
                {
                    filter = c => c.Target <= toTarget;
                }
                var list = await _unitOfWork.FundingProjectRepository.GetAllAsync(
                       filter: filter,
                       orderBy: orderBy,
                       isAscending: request.IsAscending.Value,
                       pageIndex: request.PageIndex,
                       pageSize: request.PageSize,
                       includeProperties: "Categories,Packages,SourceFiles,Packages.RewardItems,Wallet,User");
                if (list != null && list.Count() >= 0)
                {
                    var totalItems = _unitOfWork.FundingProjectRepository.GetAll(filter).Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                    IEnumerable<FundingProjectResponse> projects = _mapper.Map<IEnumerable<FundingProjectResponse>>(list);

                    PaginatedResponse<FundingProjectResponse> response = new PaginatedResponse<FundingProjectResponse>
                    {
                        PageSize = request.PageSize.Value,
                        PageIndex = request.PageIndex.Value,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = projects
                    };

                    return ResultDTO<PaginatedResponse<FundingProjectResponse>>.Success(response);
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Project Not Found.");
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<FundingProjectResponse>> GetProjectByIdAndOwner(Guid id)
        {
            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                if (authorUser is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Backer Not Found.");
                }
                User user = _mapper.Map<User>(authorUser._data);
                var project = _unitOfWork.FundingProjectRepository.GetQueryable()
                    .Include(p => p.Packages).ThenInclude(pack => pack.RewardItems)
                    .Include(p => p.SourceFiles.Where(sf => sf.IsDeleted == false))
                    .Include(p => p.Wallet).ThenInclude(w => w.BankAccount)
                    .Include(p => p.User)
                    .Include(p => p.Categories)
                    .AsSplitQuery()
                    .FirstOrDefault(p => p.Id == id);
                if (project is null)
                {
                    return ResultDTO<FundingProjectResponse>.Fail("Project not found", 404);
                }
                if (user.Id != project.UserId)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "User is not the owner of the project");
                }
                FundingProjectResponse result = _mapper.Map<FundingProjectResponse>(project);
                return ResultDTO<FundingProjectResponse>.Success(result);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw ex;
            }
        }

        public async Task DeleteFundingProject(Guid id)
        {
            try
            {
                var fundingProject = await _unitOfWork.FundingProjectRepository
                    .GetQueryable()
                    .Where(p => p.Id == id)
                    .Include(p => p.Wallet)
                    .ThenInclude(p => p.BankAccount)
                    .Include(p => p.SourceFiles)
                    .FirstOrDefaultAsync();

                if (fundingProject == null)
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Funding Project not found.");
                else if (fundingProject.Status != ProjectStatus.Pending)
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Funding Project cannot be deleted.");
                else
                {
                    //remove related wallet
                    var wallet = fundingProject.Wallet;

                    if (wallet != null)
                    {
                        _unitOfWork.WalletRepository.Remove(wallet);

                        //remove related bank account
                        var bankAccount = fundingProject.Wallet?.BankAccount ?? null;

                        if (bankAccount != null) _unitOfWork.BankAccountRepository.Remove(bankAccount);
                    }

                    fundingProject.Status = ProjectStatus.Deleted;
                    _unitOfWork.FundingProjectRepository.Update(fundingProject);

                    _unitOfWork.FundingProjectRepository.Remove(fundingProject);
                    await _unitOfWork.CommitAsync();
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
    }
}
