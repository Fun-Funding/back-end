using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;
using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
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
    public class MarketplaceService : IMarketplaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IFundingProjectService _fundingProjectService;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IAzureService _azureService;
        private readonly INotificationService _notificationService;

        public MarketplaceService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService,
            IHttpContextAccessor httpContextAccessor, IFundingProjectService fundingProjectService,
            IAzureService azureService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _fundingProjectService = fundingProjectService;
            _azureService = azureService;
            _notificationService = notificationService;
        }

        public async Task<ResultDTO<MarketplaceProjectInfoResponse>>
            CreateMarketplaceProject(MarketplaceProjectAddRequest request)
        {
            try
            {
                //check if marketplace project is deleted
                var mp = await _unitOfWork.MarketplaceRepository
                    .GetQueryable()
                    .Where(p => p.FundingProjectId == request.FundingProjectId)
                    .Include(p => p.Wallet)
                    .ThenInclude(p => p.BankAccount)
                    .FirstOrDefaultAsync();

                if (mp != null && (mp.Status == ProjectStatus.Deleted || mp.IsDeleted == true))
                {
                    var updateRequest = _mapper.Map<MarketplaceProjectUpdateRequest>(request);

                    //map bank account
                    BankAccountUpdateRequest bankAccount = new BankAccountUpdateRequest
                    {
                        Id = mp.Wallet.BankAccount.Id,
                        BankCode = request.BankAccount.BankCode,
                        BankNumber = request.BankAccount.BankNumber
                    };
                    updateRequest.BankAccount = bankAccount;

                    var response = UpdateMarketplaceProject(mp.Id, updateRequest, true).Result._data;

                    return new ResultDTO<MarketplaceProjectInfoResponse>(true, "Create successfully.",
                        response, (int)HttpStatusCode.Created);
                }
                else
                {
                    //find funding project
                    var fundingProject = await _unitOfWork.FundingProjectRepository.GetQueryable()
                        .Where(p => p.Id == request.FundingProjectId)
                        .Include(p => p.User)
                        .Include(p => p.Categories)
                        .Include(p => p.MarketplaceProject)
                        .FirstOrDefaultAsync();

                    if (fundingProject == null)
                        throw new ExceptionError((int)HttpStatusCode.NotFound, "Funding Project not found.");
                    else if (fundingProject.Status != ProjectStatus.Successful)
                        throw new ExceptionError((int)HttpStatusCode.BadRequest
                            , "The project cannot be published to marketplace if it has not complete crowdfunding on Fun&Funding platform.");
                    else if (fundingProject.MarketplaceProject != null)
                        throw new ExceptionError((int)HttpStatusCode.BadRequest,
                            "There is already a project promoted to the marketplace from the same funding project.");
                    else
                    {
                        //validate
                        var errorMessages = ValidateMarketplaceProject(request);
                        if (errorMessages != null && errorMessages.Count > 0)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, string.Join("\n", errorMessages));
                        }

                        //add files 
                        List<MarketplaceFile> files = new List<MarketplaceFile>();

                        foreach (MarketplaceFileRequest file in request.MarketplaceFiles)
                        {
                            if (file.URL.Length > 0)
                            {
                                var result = _azureService.UploadUrlSingleFiles(file.URL);

                                if (result == null)
                                {
                                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Fail to upload file");
                                }

                                MarketplaceFile media = new MarketplaceFile
                                {
                                    Name = file.Name,
                                    URL = result.Result,
                                    FileType = file.FileType,
                                    CreatedDate = DateTime.Now
                                };

                                if (file.FileType == FileType.GameFile)
                                {
                                    media.Version = "1";
                                    media.Description = "First upload";
                                }

                                files.Add(media);
                            }
                        }

                        //map project
                        var marketplaceProject = _mapper.Map<MarketplaceProject>(request);
                        marketplaceProject.MarketplaceFiles = files;
                        marketplaceProject.FundingProject = fundingProject;
                        marketplaceProject.Status = ProjectStatus.Pending;
                        marketplaceProject.CreatedDate = DateTime.Now;

                        //create a wallet
                        Wallet wallet = new Wallet
                        {
                            MarketplaceProject = marketplaceProject,
                            Balance = 0,
                            CreatedDate = DateTime.Now
                        };

                        //bank account for wallet
                        BankAccount bankAccount = new BankAccount
                        {
                            Wallet = wallet,
                            BankCode = request.BankAccount.BankCode,
                            BankNumber = request.BankAccount.BankNumber,
                            CreatedDate = DateTime.Now
                        };

                        marketplaceProject.Wallet = wallet;
                        marketplaceProject.Wallet.BankAccount = bankAccount;

                        //save to db
                        await _unitOfWork.MarketplaceRepository.AddAsync(marketplaceProject);
                        await _unitOfWork.WalletRepository.AddAsync(wallet);
                        await _unitOfWork.BankAccountRepository.AddAsync(bankAccount);

                        await _unitOfWork.CommitAsync();

                        //response
                        var response = _mapper.Map<MarketplaceProjectInfoResponse>(marketplaceProject);
                        return new ResultDTO<MarketplaceProjectInfoResponse>(true, "Create successfully.",
                            response, (int)HttpStatusCode.Created);
                    }
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

        public async Task<ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>> GetAllMarketplaceProject
            (ListRequest request, List<Guid>? categoryIds, List<ProjectStatus>? statusList, decimal? fromPrice, decimal? toPrice)
        {

            try
            {
                var filters = new List<Func<IQueryable<MarketplaceProject>, IQueryable<MarketplaceProject>>>();
                Expression<Func<MarketplaceProject, object>> orderBy = c => c.CreatedDate;

                /*if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "name":
                            orderBy = c => c.Name;
                            break;
                        case "category":
                            orderBy = c => c.FundingProject.Categories
                                .OrderBy(category => category.Name)
                                .FirstOrDefault().Name;
                            break;
                        default:
                            break;
                    }
                }*/

                //category filter
                if (categoryIds != null && categoryIds.Any())
                {
                    filters.Add(query => query.Where(c => c.FundingProject.Categories.Any(category => categoryIds.Contains(category.Id))));
                }

                //search
                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    string searchLower = request.SearchValue.ToLower();
                    filters.Add(query => query.Where(u => u.Name != null && u.Name.ToLower().Contains(searchLower)));
                }

                // statuses filter
                if (statusList != null && statusList.Any())
                {
                    filters.Add(query => query.Where(c => statusList.Contains(c.Status)));
                }

                // price filter
                if (fromPrice.HasValue)
                {
                    filters.Add(query => query.Where(c => c.Price >= fromPrice.Value));
                }

                if (toPrice.HasValue)
                {
                    filters.Add(query => query.Where(c => c.Price <= toPrice.Value));
                }

                var list = await _unitOfWork.MarketplaceRepository.GetAllCombinedFilterAsync(
                   filters: filters,
                   orderBy: orderBy,
                   isAscending: request.IsAscending.Value,
                   includeProperties: "MarketplaceFiles,FundingProject.User,FundingProject.Categories,Wallet,Wallet.BankAccount",
                   pageIndex: request.PageIndex,
                   pageSize: request.PageSize);

                var totalItems = _unitOfWork.MarketplaceRepository
                    .GetAllCombinedFilterAsync(filters: filters)
                    .Result.ToList().Count();

                var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);

                IEnumerable<MarketplaceProjectInfoResponse> marketplaceProjects =
                    _mapper.Map<IEnumerable<MarketplaceProjectInfoResponse>>(list);

                PaginatedResponse<MarketplaceProjectInfoResponse> response = new PaginatedResponse<MarketplaceProjectInfoResponse>
                {
                    PageSize = request.PageSize.Value,
                    PageIndex = request.PageIndex.Value,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = marketplaceProjects
                };

                return ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>.Success(response);

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

        public async Task<ResultDTO<MarketplaceProjectInfoResponse>> GetMarketplaceProjectById(Guid id)
        {
            try
            {
                //var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetQueryable()
                //    .Where(p => p.Id == id && p.IsDeleted == false)
                //    .Include(p => p.MarketplaceFiles)
                //    .Include(p => p.FundingProject.Categories)
                //    .Include(p => p.FundingProject)
                //    .ThenInclude(p => p.User)
                //    .Include(p => p.Wallet)
                //    .ThenInclude(p => p.BankAccount)
                //    .FirstOrDefaultAsync();

                var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetNonDeletedMarketplaceProjectById(id);

                if (marketplaceProject == null)
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace Project not found.");
                else
                {
                    if (marketplaceProject.MarketplaceFiles != null)
                    {
                        var existingFiles = marketplaceProject.MarketplaceFiles.ToList();
                        marketplaceProject.MarketplaceFiles = GetNonDeletedFiles(existingFiles);
                    }

                    var response = _mapper.Map<MarketplaceProjectInfoResponse>(marketplaceProject);

                    return ResultDTO<MarketplaceProjectInfoResponse>.Success(response);
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

        public async Task DeleteMarketplaceProject(Guid id)
        {
            try
            {
                //var marketPlaceProject = await _unitOfWork.MarketplaceRepository
                //    .GetQueryable()
                //    .Where(p => p.Id == id)
                //    .Include(p => p.MarketplaceFiles)
                //    .Include(p => p.ProjectCoupons)
                //    .Include(p => p.Wallet)
                //    .ThenInclude(p => p.BankAccount)
                //    .FirstOrDefaultAsync();

                var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetNonDeletedMarketplaceProjectById(id);

                if (marketplaceProject == null)
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace Project not found.");
                else if (marketplaceProject.Status != ProjectStatus.Pending)
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Marketplace Project cannot be deleted.");
                else
                {
                    //remove related files
                    if (marketplaceProject.MarketplaceFiles != null
                        && marketplaceProject.MarketplaceFiles.Count > 0)
                    {
                        _unitOfWork.MarketplaceFileRepository.RemoveRange(marketplaceProject.MarketplaceFiles);
                    }

                    //remove related coupons
                    if (marketplaceProject.ProjectCoupons != null
                        && marketplaceProject.ProjectCoupons.Count > 0)
                    {
                        _unitOfWork.ProjectCouponRepository.RemoveRange(marketplaceProject.ProjectCoupons);
                    }

                    //remove related wallet
                    var wallet = marketplaceProject.Wallet;

                    if (wallet != null)
                    {
                        _unitOfWork.WalletRepository.Remove(wallet);

                        //remove related bank account
                        var bankAccount = marketplaceProject.Wallet.BankAccount;

                        if (bankAccount != null) _unitOfWork.BankAccountRepository.Remove(bankAccount);
                    }

                    marketplaceProject.Status = ProjectStatus.Deleted;
                    _unitOfWork.MarketplaceRepository.Update(marketplaceProject);

                    _unitOfWork.MarketplaceRepository.Remove(marketplaceProject);
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


        public async Task<ResultDTO<MarketplaceProjectInfoResponse>>
            UpdateMarketplaceProject(Guid id, MarketplaceProjectUpdateRequest request, bool? isDeleted = null)
        {
            try
            {
                //MarketplaceProject marketplaceProject = await _unitOfWork.MarketplaceRepository
                //                        .GetQueryable()
                //                        .Where(p => p.Id == id)
                //                        .Include(p => p.MarketplaceFiles)
                //                        .Include(p => p.FundingProject.Categories)
                //                        .Include(p => p.FundingProject)
                //                        .ThenInclude(p => p.User)
                //                        .Include(p => p.Wallet)
                //                        .ThenInclude(p => p.BankAccount)
                //                        .FirstOrDefaultAsync();

                MarketplaceProject? marketplaceProject = await _unitOfWork.MarketplaceRepository.GetMarketplaceProjectById(id);

                if (marketplaceProject == null)
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace Project not found.");
                else
                {
                    if (isDeleted != null && isDeleted == true)
                    {
                        marketplaceProject.Status = ProjectStatus.Pending;
                        marketplaceProject.IsDeleted = false;
                        marketplaceProject.DeletedAt = null;
                        marketplaceProject.CreatedDate = DateTime.Now;

                        //restore wallet
                        var wallet = marketplaceProject.Wallet;
                        if (wallet != null)
                        {
                            marketplaceProject.Wallet.IsDeleted = false;
                            marketplaceProject.Wallet.DeletedAt = null;
                            marketplaceProject.Wallet.CreatedDate = DateTime.Now;

                            //restore bank account
                            var bankAccount = marketplaceProject.Wallet.BankAccount;
                            if (bankAccount != null)
                            {
                                marketplaceProject.Wallet.BankAccount.IsDeleted = false;
                                marketplaceProject.Wallet.BankAccount.DeletedAt = null;
                                marketplaceProject.Wallet.BankAccount.CreatedDate = DateTime.Now;
                            }
                        }
                    }

                    if (marketplaceProject.Status != ProjectStatus.Deleted
                        && marketplaceProject.Status != ProjectStatus.Reported
                        && marketplaceProject.Status != ProjectStatus.Rejected)
                    {
                        //validate
                        var errorMessages = ValidateMarketplaceProject(request);
                        if (errorMessages != null && errorMessages.Count > 0)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, string.Join("\n", errorMessages));
                        }

                        var marketplaceFiles = marketplaceProject.MarketplaceFiles;

                        //remove deleted files
                        var filesToDelete = _mapper.Map<IEnumerable<MarketplaceFile>>(request.ExistingFiles);
                        if (filesToDelete != null && filesToDelete.Count() > 0)
                        {
                            foreach (var file in filesToDelete)
                            {
                                if (file.IsDeleted) _unitOfWork.MarketplaceFileRepository.DeleteMarketplaceFile(file);
                            }
                        }

                        //files to be update
                        var updateFiles = request.MarketplaceFiles;
                        if (updateFiles != null && updateFiles.Count() > 0)
                        {
                            var filesToUpdate = AddFiles(updateFiles, id);

                            marketplaceFiles = marketplaceFiles.Concat(filesToUpdate).ToList();
                        }

                        _mapper.Map(request, marketplaceProject);

                        if (marketplaceProject.Wallet != null)
                        {
                            var bankAccount = marketplaceProject.Wallet.BankAccount;
                            _mapper.Map(request.BankAccount, bankAccount);

                            _unitOfWork.BankAccountRepository.Update(bankAccount);

                            marketplaceProject.Wallet.BankAccount = bankAccount;
                        }

                        marketplaceProject.MarketplaceFiles = marketplaceFiles;

                        _unitOfWork.MarketplaceRepository.Update(marketplaceProject);
                        await _unitOfWork.CommitAsync();

                        //return non-deleted files only
                        marketplaceProject.MarketplaceFiles = GetNonDeletedFiles(marketplaceFiles.ToList());

                        var response = _mapper.Map<MarketplaceProjectInfoResponse>(marketplaceProject);

                        return ResultDTO<MarketplaceProjectInfoResponse>.Success(response);
                    }
                    else
                        throw new ExceptionError((int)HttpStatusCode.BadRequest,
                            $"Marketplace Project cannot be updated when in status {marketplaceProject.Status}.");
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

        public async Task<ResultDTO<MarketplaceProjectInfoResponse>> UpdateMarketplaceProjectStatus
            (Guid id, ProjectStatus status, string? note)
        {
            try
            {
                //var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetQueryable()
                //    .Where(p => p.Id == id)
                //    .Include(p => p.MarketplaceFiles)
                //    .Include(p => p.FundingProject.Categories)
                //    .Include(p => p.FundingProject)
                //    .ThenInclude(p => p.User)
                //    .FirstOrDefaultAsync();

                var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetMarketplaceProjectById(id);

                //pending status change list
                List<ProjectStatus> pendingChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Processing,
                        ProjectStatus.Rejected,
                        ProjectStatus.Deleted
                    };

                //rejected status change list
                List<ProjectStatus> rejectedChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Deleted
                    };

                //processing status change list
                List<ProjectStatus> processingChangelist = new List<ProjectStatus>()
                    {
                        ProjectStatus.Reported
                    };

                bool isChanged = false;

                if (marketplaceProject != null)
                {
                    //change status from pending
                    if (marketplaceProject.Status == ProjectStatus.Pending && pendingChangelist.Contains(status))
                    {
                        marketplaceProject.Status = status;
                        marketplaceProject.Note = note;
                        isChanged = true;
                    }
                    //change status from rejected
                    else if (marketplaceProject.Status == ProjectStatus.Rejected && rejectedChangelist.Contains(status))
                    {
                        marketplaceProject.Status = status;
                        isChanged = true;
                    }
                    //change status from processing
                    else if (marketplaceProject.Status == ProjectStatus.Processing && processingChangelist.Contains(status))
                    {
                        marketplaceProject.Status = status;
                        isChanged = true;
                    }

                    if (isChanged)
                    {
                        _unitOfWork.MarketplaceRepository.Update(marketplaceProject);
                        await _unitOfWork.CommitAsync();

                        var response = _mapper.Map<MarketplaceProjectInfoResponse>(marketplaceProject);

                        // NOTIFICATION
                        // 1. get recipientsIds
                        List<Guid> recipientsId = new List<Guid>();
                        recipientsId.Add(marketplaceProject.FundingProject.User.Id);
                        //recipientsId.Add(Guid.Parse("f766c910-4f6a-421e-a1a3-61534e6005c3"));
                        // 2. initiate new Notification object
                        var notification = new Notification
                        {
                            Id = new Guid(),
                            Date = DateTime.Now,
                            Message = $"has updated status project <b>{marketplaceProject.Name}</b>",
                            NotificationType = NotificationType.MarketplaceProjectStatus,
                            Actor = new { Id = new Guid(), UserName = "Admin", Avatar = "" },
                            ObjectId = marketplaceProject.Id,
                        };

                        await _notificationService.SendNotification(notification, recipientsId);

                        return ResultDTO<MarketplaceProjectInfoResponse>.Success(response);
                    }
                    else throw new ExceptionError(
                        (int)HttpStatusCode.BadRequest,
                        $"Marketplace Project with status {marketplaceProject.Status} cannot be changed to {status}.");
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace Project Not Found.");
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

        //validation
        private List<string> ValidateCommonFields(dynamic request)
        {
            try
            {
                List<string> errorMessages = new List<string>();

                if (string.IsNullOrEmpty(request.Name))
                {
                    errorMessages.Add("Name is required.");
                }

                if (string.IsNullOrEmpty(request.Description))
                {
                    errorMessages.Add("Description is required.");
                }

                if (string.IsNullOrEmpty(request.Introduction))
                {
                    errorMessages.Add("Introduction is required.");
                }

                if (request.Price < 1000)
                {
                    errorMessages.Add("Invalid price.");
                }

                return errorMessages;
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

        private List<string> ValidateMarketplaceProject(MarketplaceProjectAddRequest request)
        {
            var errorMessages = ValidateCommonFields(request);
            if (request.MarketplaceFiles.Count <= 0)
            {
                errorMessages.Add("Missing file(s).");
            }

            List<MarketplaceProject> list = _unitOfWork.MarketplaceRepository.GetAll().ToList();
            if (list.All(p => p.Name == request.Name))
            {
                errorMessages.Add("Name cannot be duplicated.");
            }

            return errorMessages;
        }

        private List<string> ValidateMarketplaceProject(MarketplaceProjectUpdateRequest request)
        {
            return ValidateCommonFields(request);
        }

        //add files
        private List<MarketplaceFile> AddFiles(List<MarketplaceFileRequest> marketplaceFiles, Guid id)
        {
            List<MarketplaceFile> files = new List<MarketplaceFile>();

            foreach (MarketplaceFileRequest file in marketplaceFiles)
            {
                if (file.URL.Length > 0)
                {
                    var result = _azureService.UploadUrlSingleFiles(file.URL);

                    if (result == null)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Fail to upload file");
                    }

                    MarketplaceFile media = new MarketplaceFile
                    {
                        Name = file.Name,
                        URL = result.Result,
                        Version = file.Version,
                        Description = file.Description,
                        FileType = file.FileType,
                        CreatedDate = DateTime.Now,
                        MarketplaceProjectId = id
                    };

                    files.Add(media);
                }
            }

            return files;
        }

        //get non-deleted files
        private List<MarketplaceFile> GetNonDeletedFiles(List<MarketplaceFile> marketplaceFiles)
        {
            List<MarketplaceFile> files = new List<MarketplaceFile>();

            foreach (MarketplaceFile file in marketplaceFiles)
            {
                if (file.IsDeleted == false)
                {
                    files.Add(file);
                }
            }

            return files;
        }

        //get marketplace project wallet
        private async Task<Wallet?> GetMarketplaceProjectWallet(Guid marketplaceProjectId)
        {
            return await _unitOfWork.WalletRepository
                .GetQueryable()
                .Include(w => w.MarketplaceProject)
                .Where(w => w.MarketplaceProject.Id == marketplaceProjectId)
                .FirstOrDefaultAsync();
        }

        //get bank account by wallet id
        private async Task<BankAccount?> GetBankAccountById(Guid id)
        {
            return await _unitOfWork.BankAccountRepository
                .GetQueryable()
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ResultDTO<List<MarketplaceProjectInfoResponse>>> GetTop3MostPurchasedOngoingMarketplaceProject()
        {
            try
            {
                var projects = await _unitOfWork.MarketplaceRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(p => p.MarketplaceFiles)
                    .Include(p => p.DigitalKeys)
                    .Include(p => p.FundingProject.Categories)
                    .Include(p => p.FundingProject)
                        .ThenInclude(p => p.User)
                    .Where(mp => mp.Status == ProjectStatus.Processing)
                    .OrderByDescending(p => p.DigitalKeys.Count())
                    .Take(3)
                    .ToListAsync();

                List<MarketplaceProjectInfoResponse> result = _mapper.Map<List<MarketplaceProjectInfoResponse>>(projects);

                return ResultDTO<List<MarketplaceProjectInfoResponse>>.Success(result, "Marketplace Project Found!");
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

        public async Task<ResultDTO<List<MarketplaceProjectInfoResponse>>> GetTop4MostPurchasedOngoingMarketplaceProject()
        {
            try
            {
                var projects = await _unitOfWork.MarketplaceRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(p => p.MarketplaceFiles)
                    .Include(p => p.DigitalKeys)
                    .Include(p => p.FundingProject.Categories)
                    .Include(p => p.FundingProject)
                        .ThenInclude(p => p.User)
                    .Where(mp => mp.Status == ProjectStatus.Processing)
                    .OrderByDescending(p => p.DigitalKeys.Count())
                    .Take(4)
                    .ToListAsync();

                List<MarketplaceProjectInfoResponse> result = _mapper.Map<List<MarketplaceProjectInfoResponse>>(projects);

                return ResultDTO<List<MarketplaceProjectInfoResponse>>.Success(result, "Marketplace Project Found!");
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

        public async Task<ResultDTO<decimal>> CountPlatformProjects()
        {
            try
            {
                var marketplaceProjects = await _unitOfWork.MarketplaceRepository.GetQueryable().CountAsync();
                var fundingProjects = await _unitOfWork.FundingProjectRepository.GetQueryable().CountAsync();

                return ResultDTO<decimal>.Success(marketplaceProjects + fundingProjects, "Found total project!");
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

        public async Task<ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>> GetGameOwnerMarketplaceProject(ListRequest request)
        {

            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                if (authorUser is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Game Owner Not Found.");
                }
                User user = _mapper.Map<User>(authorUser._data);

                Expression<Func<MarketplaceProject, bool>> filter = u => u.FundingProject.UserId == user.Id;
                Expression<Func<MarketplaceProject, object>> orderBy = c => c.CreatedDate;

                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "name":
                            orderBy = c => c.Name;
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    filter = c => c.Name.ToLower().Contains(request.SearchValue.ToLower());
                }

                var list = await _unitOfWork.MarketplaceRepository.GetAllAsync(
                   filter: filter,
                   orderBy: orderBy,
                   isAscending: request.IsAscending.Value,
                   includeProperties: "MarketplaceFiles,FundingProject.User,FundingProject.Categories,Wallet",
                   pageIndex: request.PageIndex,
                   pageSize: request.PageSize);

                var totalItems = _unitOfWork.MarketplaceRepository.GetAll(filter).Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                IEnumerable<MarketplaceProjectInfoResponse> marketplaceProjects =
                    _mapper.Map<IEnumerable<MarketplaceProjectInfoResponse>>(list);

                PaginatedResponse<MarketplaceProjectInfoResponse> response = new PaginatedResponse<MarketplaceProjectInfoResponse>
                {
                    PageSize = request.PageSize.Value,
                    PageIndex = request.PageIndex.Value,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = marketplaceProjects
                };

                return ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>.Success(response);

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

        public async Task<ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>> GetBackerPurchasedMarketplaceProject(ListRequest request)
        {

            try
            {
                var authorUser = _userService.GetUserInfo().Result;
                if (authorUser is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Game Owner Not Found.");
                }
                User user = _mapper.Map<User>(authorUser._data);

                List<Order> orderList = _unitOfWork.OrderRepository
                .GetQueryable()
                .Where(pb => pb.UserId.Equals(user.Id))
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.DigitalKey)
                .ThenInclude(dk => dk.MarketplaceProject)
                .ToList();

                // Extract Marketplace IDs
                var marketplaceProjectIds = orderList
                .Where(o => o.OrderDetails != null)
                .SelectMany(o => o.OrderDetails)
                .Where(od => od.DigitalKey != null)
                .Select(od => od.DigitalKey)
                .Where(dk => dk.MarketplaceProject != null)
                .Select(dk => dk.MarketplaceProject.Id)
                .Distinct()
                .ToList();

                Expression<Func<MarketplaceProject, bool>> filter = u => marketplaceProjectIds.Contains(u.Id);
                Expression<Func<MarketplaceProject, object>> orderBy = c => c.CreatedDate;

                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "name":
                            orderBy = c => c.Name;
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    filter = c => c.Name.ToLower().Contains(request.SearchValue.ToLower());
                }

                var list = await _unitOfWork.MarketplaceRepository.GetAllAsync(
                   filter: filter,
                   orderBy: orderBy,
                   isAscending: request.IsAscending.Value,
                   includeProperties: "MarketplaceFiles,FundingProject.User,FundingProject.Categories,Wallet",
                   pageIndex: request.PageIndex,
                   pageSize: request.PageSize);

                var totalItems = _unitOfWork.MarketplaceRepository.GetAll(filter).Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                IEnumerable<MarketplaceProjectInfoResponse> marketplaceProjects =
                    _mapper.Map<IEnumerable<MarketplaceProjectInfoResponse>>(list);

                PaginatedResponse<MarketplaceProjectInfoResponse> response = new PaginatedResponse<MarketplaceProjectInfoResponse>
                {
                    PageSize = request.PageSize.Value,
                    PageIndex = request.PageIndex.Value,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = marketplaceProjects
                };

                return ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>.Success(response);

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
