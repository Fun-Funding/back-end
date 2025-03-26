using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.NotificationDTO;
using Fun_Funding.Application.ViewModel.PackageBackerDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class PackageBackerService : IPackageBackerService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;
        private IMapper _mapper;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IAzureService _azureService;
        public PackageBackerService(IUnitOfWork unitOfWork, ITransactionService transactionService, IUserService userService, IMapper mapper,
            INotificationService notificationService, IAzureService azureService)
        {
            _unitOfWork = unitOfWork;
            _transactionService = transactionService;
            _userService = userService;
            _mapper = mapper;
            _notificationService = notificationService;
            _azureService = azureService;
        }
        public async Task<ResultDTO<PackageBackerResponse>> DonateFundingProject(PackageBackerRequest packageBackerRequest)
        {
            if (packageBackerRequest == null)
                return ResultDTO<PackageBackerResponse>.Fail("Invalid request data");

            try
            {
               var authorUser = _userService.GetUserInfo().Result;
                User mapUser = _mapper.Map<User>(authorUser._data);
                
                User user = _unitOfWork.UserRepository.GetById(mapUser.Id);
                if (authorUser is null)
                    return ResultDTO<PackageBackerResponse>.Fail("can not found user");
                
                var package = await _unitOfWork.PackageRepository.GetByIdAsync(packageBackerRequest.PackageId);
                if (package == null)
                    return ResultDTO<PackageBackerResponse>.Fail("Package not found!");

                var wallet = await _unitOfWork.WalletRepository.GetAsync(w => w.Backer.Id == user.Id);
                if (wallet == null)
                    return ResultDTO<PackageBackerResponse>.Fail("Wallet not found");

                if (package.LimitQuantity == 0 && package.PackageTypes.Equals(PackageType.FixedPackage))
                    return ResultDTO<PackageBackerResponse>.Fail("Package is currently out of quantity!");

                if (_unitOfWork.FundingProjectRepository.GetById(package.ProjectId).Status != ProjectStatus.Processing)
                    return ResultDTO<PackageBackerResponse>.Fail("Project is currently cannot be donated to!");

                if (package.PackageTypes.Equals(PackageType.FixedPackage))
                    packageBackerRequest.DonateAmount = package.RequiredAmount;
                else
                {
                    if (packageBackerRequest.DonateAmount <= 0)
                        return ResultDTO<PackageBackerResponse>.Fail("Invalid donate amount");
                }
                //add project balance
                var project = _unitOfWork.FundingProjectRepository.GetById(package.ProjectId);
                project.Balance += packageBackerRequest.DonateAmount;
                _unitOfWork.FundingProjectRepository.Update(project);
                // add donation
                var packageBacker = new PackageBacker
                {
                    UserId = user.Id,
                    PackageId = packageBackerRequest.PackageId,
                    User = user,
                    Package = package,
                    DonateAmount = packageBackerRequest.DonateAmount,
                    CreatedDate = DateTime.Now
                };
                // add donation amount to project wallet
                var projectWallet = _unitOfWork.WalletRepository.GetQueryable()
                    .Include(w => w.FundingProject)
                    .FirstOrDefault(w => w.FundingProject.Id == package.ProjectId);

                if (wallet.Balance > 0 && wallet.Balance >= packageBackerRequest.DonateAmount)
                {
                    wallet.Balance -= packageBackerRequest.DonateAmount;
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Backer wallet is not enough money for donation! Please charge more");
                }
                projectWallet.Balance += packageBackerRequest.DonateAmount;


                Package donatedPack = _unitOfWork.PackageRepository.GetById(packageBackerRequest.PackageId);
                donatedPack.LimitQuantity -= 1;
                await _unitOfWork.PackageBackerRepository.AddAsync(packageBacker);
                _unitOfWork.PackageRepository.Update(donatedPack);
                _unitOfWork.WalletRepository.Update(projectWallet);
                _unitOfWork.WalletRepository.Update(wallet);
                // add transaction
                var description = $"Donation to package: {package.Name}";
                await _transactionService.CreateTransactionAsync(
                    totalAmount: packageBackerRequest.DonateAmount,
                    description: description,
                    transactionType: TransactionTypes.PackageDonation,
                    packageId: packageBackerRequest.PackageId,
                    walletId: user.Wallet.Id
                );

                await _unitOfWork.CommitAsync();

                //var response = new PackageBackerResponse
                //{

                //};

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
                    Message = $"donated to project <b>{project.Name}</b>",
                    NotificationType = NotificationType.FundingProjectInteraction,
                    Actor = new { user.Id, user.UserName, authorUser._data.Avatar},
                    ObjectId = project.Id,
                };

                await _notificationService.SendNotification(notification, recipientsId);


                return ResultDTO<PackageBackerResponse>.Success(null, "Donation successfully added!");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultDTO<PackageBackerResponse>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ResultDTO<List<DonationResponse>>> ViewDonationById(Guid id)
        {
            try
            {
                var listById = _unitOfWork.PackageBackerRepository.GetQueryable()
                    .Include(x => x.Package)
                    .Include(x => x.User)
                    .Where(x => x.UserId == id)
                    .ToList();
                if (listById is null)
                {
                    return ResultDTO<List<DonationResponse>>.Fail("There are no donation found with this id");
                }

                var response = listById.Select(x => new DonationResponse
                {
                    UserName = x.User.FullName,
                    CreateDate = x.CreatedDate,
                    DonateAmount = x.DonateAmount,
                    PackageName = x.Package.Name,
                    Types = x.Package.PackageTypes,

                }).ToList();

                return ResultDTO<List<DonationResponse>>.Success(response, "donation by id");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<List<PackageBackerGroupedResponse>>> GetGroupedPackageBackersAsync(Guid projectId)
        {
            var packageBackers =  _unitOfWork.PackageBackerRepository.GetQueryable()
                .Include(x => x.EvidenceImages)
            .Where(pb => pb.Package.Project.Id == projectId) // Filter by project ID
            .GroupBy(pb => pb.CreatedDate.Date) // Group by CreatedDate (date only)
            .Select(group => new PackageBackerGroupedResponse
            {
            CreatedDate = group.Key,

            TotalDonateAmount = group.Sum(pb => pb.DonateAmount)
            })
            .ToList();

            return ResultDTO<List<PackageBackerGroupedResponse>>.Success(packageBackers, "");
        }

        public async Task<ResultDTO<List<PackageBackerCountResponse>>> GetPackageBackerGroups(Guid projectId)
        {
            try
            {
                var groupedResult = _unitOfWork.PackageBackerRepository.GetQueryable()
                    .Where(pb => pb.Package.Project.Id == projectId)
                    .GroupBy(pb => new { pb.PackageId, pb.Package.Name }) // Group by PackageId and PackageName
                    .Select(group => new PackageBackerCountResponse
                    {
                        PackageId = group.Key.PackageId,
                        PackageName = group.Key.Name,
                        Count = group.Count() // Count the number of records in each group
                    })
                    .ToList();

                return ResultDTO<List<PackageBackerCountResponse>>.Success(groupedResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<IEnumerable<object>>> GetProjectBacker(Guid projectId)
        {
            try
            {
                var groupedResult = await _unitOfWork.PackageBackerRepository.GetQueryable()
                    .Include(pb => pb.User)
                    .Include(pb => pb.Package)
                        .ThenInclude(p => p.RewardItems)
                    .Include(pb => pb.EvidenceImages)
                    .Where(pb => pb.Package.Project.Id == projectId)
                    .Select(pb => new
                    {
                        pb.User.UserName,
                        pb.Id,
                        pb.Package.Name,
                        pb.User.Address,
                        pb.User.Email,
                        pb.DonateAmount,
                        pb.CreatedDate,
                        EvidenceImages =  pb.EvidenceImages.Select(ei => ei.Url).ToList() ,
                        RewardItems = pb.Package.RewardItems.Select(ri => new 
                        {
                            ri.Name,
                            ri.ImageUrl,
                            ri.Description,
                            ri.Quantity
                        }).ToList()
                    })
                    .OrderByDescending(pb => pb.CreatedDate)
                    .ToListAsync(); // Use async to avoid blocking

                return ResultDTO<IEnumerable<object>>.Success(groupedResult);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching project backers: {ex.Message}");
            }
        }

        public async Task<ResultDTO<IEnumerable<object>>> GetBackerDonations(Guid fundingProjectId)
        {
            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                if(authorUser == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                User mapUser = _mapper.Map<User>(authorUser._data);

                User user = _unitOfWork.UserRepository.GetById(mapUser.Id);
                var listById = _unitOfWork.PackageBackerRepository.GetQueryable()
                    .Include(x => x.Package)
                        .ThenInclude(x => x.RewardItems)
                    .Include(x => x.User)
                    .Where(x => x.UserId == user.Id && x.Package.ProjectId == fundingProjectId)
                    .ToList();
                if (listById is null)
                {
                    return ResultDTO<IEnumerable<object>>.Fail("There are no donation found with this id");
                }

                var response = listById.Select(x => new
                {
                    UserId = x.UserId,
                    UserName = x.User?.FullName ?? "Unknown User",
                    CreateDate = x.CreatedDate,
                    DonateAmount = x.DonateAmount,
                    PackageId = x.PackageId,
                    PackageUrl = x.Package?.Url ?? string.Empty,
                    PackageName = x.Package?.Name ?? "Unknown Package",
                    PackageDescription = x.Package?.Description ?? string.Empty,
                    RewardItems = x.Package?.RewardItems != null
                    ? x.Package.RewardItems.Select(reward => new
                    {
                        RewardId = reward.Id,
                        ImageUrl = reward.ImageUrl,
                        RewardName = reward.Name,
                        Description = reward.Description,
                        Quantity = reward.Quantity
                    }).ToList()
                    : null,
                    Types = x.Package?.PackageTypes ?? null,
                    ProjectId = x.Package?.ProjectId,
                }).ToList();

                return ResultDTO<IEnumerable<object>>.Success(response, "Get Donations Successfully");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<object>> UploadEvidence(Guid id, List<IFormFile> formFiles)
        {
            try
            {
                PackageBacker packageBacker = _unitOfWork.PackageBackerRepository.GetQueryable().
                    Include(packageBacker => packageBacker.EvidenceImages).
                    FirstOrDefault(p => p.Id == id);
                if (packageBacker == null) {
                    return ResultDTO<object>.Fail("No backers found");
                }
                foreach (var file in formFiles)
                {
                    var url = _azureService.UploadUrlSingleFiles(file);
                    var evidenceImage = new EvidenceImage
                    {
                        Url = url.Result,
                        CreatedDate = DateTime.Now
                    };
                    packageBacker.EvidenceImages.Add(evidenceImage);

                }
                
                _unitOfWork.Commit();
                var response = new
                {
                    packageBacker.Id,
                    packageBacker.EvidenceImages,
                };
                return ResultDTO<object>.Success(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<IEnumerable<object>>> GetGroupDonators(Guid projectId)
        {
            var packageBackers = _unitOfWork.PackageBackerRepository.GetQueryable()
               .Include(x => x.EvidenceImages)
           .Where(pb => pb.Package.Project.Id == projectId)
           .GroupBy(pb => pb.User)
           .Select(group => 
           (object)new 
           { 
               UserId = group.Key.Id, 
               UserName = group.Key.FullName,
               Email = group.Key.Email,
               TotalAmount = group.Sum(pb => pb.DonateAmount),
           })
           .ToList();
           

            return ResultDTO<IEnumerable<object>>.Success(packageBackers, "");
        }
    }
}
