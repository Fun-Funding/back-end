using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.TransactionDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class SystemWalletService : ISystemWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public SystemWalletService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<ResultDTO<SystemWallet>> CreateWallet()
        {
            try
            {
                var wallet = new SystemWallet
                {
                    Id = new Guid(),
                    CreatedDate = DateTime.Now,
                    TotalAmount = 0
                };
                _unitOfWork.SystemWalletRepository.Add(wallet);
                await _unitOfWork.CommitAsync();
                return ResultDTO<SystemWallet>.Success(wallet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<object>> GetDashboardCategories()
        {
            try
            {
                var categories = await _unitOfWork.CategoryRepository.GetAllAsync();

                var totalProjects = _unitOfWork.FundingProjectRepository.GetAll().Count();

                var response = new List<object>();

                foreach (var category in categories)
                {
                    var fundingProjects = await _unitOfWork.FundingProjectRepository
                        .GetQueryable()
                        .AsNoTracking()
                        .Include(p => p.Categories)
                        .Where(p => p.Categories.Any(pc => pc.Name.Equals(category.Name)))
                        .ToListAsync();

                    response.Add(new
                    {
                        CategoryName = category.Name,
                        PercentageUsed = ((double)fundingProjects.Count / totalProjects) * 100,
                        ProjectCount = fundingProjects.Count
                    });
                }

                response = response
                    .OrderByDescending(r => (double?)r.GetType().GetProperty("PercentageUsed")?.GetValue(r) ?? 0)
                    .ToList();

                return ResultDTO<object>.Success(response);
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

        public async Task<ResultDTO<object>> GetDashboardFundingProjects()
        {
            try
            {
                ProjectStatus[] statuses =
                {
                    ProjectStatus.FundedSuccessful,
                    ProjectStatus.Processing,
                    ProjectStatus.Successful,
                    ProjectStatus.Failed,
                    ProjectStatus.Reported
                };

                var response = new List<object>();

                foreach (var status in statuses)
                {
                    Expression<Func<FundingProject, bool>> filter = p => p.Status == status;

                    var fundingProjects = await _unitOfWork.FundingProjectRepository.GetAllAsync(filter);

                    response.Add(new
                    {
                        Status = status.ToString(),
                        Count = fundingProjects.Count()
                    });
                }

                return ResultDTO<object>.Success(response);
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

        public ResultDTO<object> GetDashboardIncome()
        {
            try
            {
                DateTime today = DateTime.Now.Date;
                DateTime startDate = today.AddMonths(-1);

                // all dates in the range
                var allDates = Enumerable.Range(0, (today - startDate).Days + 1)
                                         .Select(offset => startDate.AddDays(offset)) // each date
                                         .ToList();

                Expression<Func<Transaction, bool>> filter = t =>
                t.CreatedDate.Date >= startDate && t.CreatedDate.Date <= today
                && t.TransactionType == TransactionTypes.CommissionFee;

                var transactions = _unitOfWork.TransactionRepository.GetAll(filter);

                var transactionSummary = transactions
                    .GroupBy(t => t.CreatedDate.Date)
                    .Select(t => new
                    {
                        Date = t.Key,
                        TotalAmount = t.Sum(t => t.TotalAmount)
                    });

                var response = allDates.GroupJoin(
                    transactionSummary,
                    date => date,                       // key from all dates
                    summary => summary.Date,            // key from transactions summary
                    (date, summaries) => new
                    {
                        Date = date,
                        TotalAmount = summaries.Any()   // no transactions -> set 0
                            ? summaries.Sum(s => s.TotalAmount) : 0
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return ResultDTO<object>.Success(response);
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

        public async Task<ResultDTO<object>> GetDashboardMarketplaceProjects()
        {
            try
            {
                ProjectStatus[] statuses =
                {
                    ProjectStatus.Processing,
                    ProjectStatus.Reported
                };

                var response = new List<object>();

                foreach (var status in statuses)
                {
                    Expression<Func<MarketplaceProject, bool>> filter = p => p.Status == status;

                    var marketplaceProjects = await _unitOfWork.MarketplaceRepository.GetAllDeletedNoPaginationAsync(filter);

                    response.Add(new
                    {
                        Status = status.ToString(),
                        Count = marketplaceProjects.Count()
                    });
                }

                return ResultDTO<object>.Success(response);
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

        public async Task<ResultDTO<object>> GetDashboardMetrics()
        {
            try
            {
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.UtcNow.Month, 1);

                var filterFunding = new List<Func<IQueryable<FundingProject>, IQueryable<FundingProject>>>();
                filterFunding.Add(query => query.Where(c => c.CreatedDate >= firstDayOfMonth));
                var filterMarketplace = new List<Func<IQueryable<MarketplaceProject>, IQueryable<MarketplaceProject>>>();
                filterMarketplace.Add(query => query.Where(c => c.CreatedDate >= firstDayOfMonth));

                var users = await _unitOfWork.UserRepository.GetQueryable().Where(u => u.CreatedDate >= firstDayOfMonth).AsNoTracking().ToListAsync();
                var fundingProjects = await _unitOfWork.FundingProjectRepository.GetAllCombinedFilterAsync(filterFunding);
                var marketplaceProjects = await _unitOfWork.MarketplaceRepository.GetAllCombinedFilterAsync(filterMarketplace);

                var response = new
                {
                    NumberOfUsers = users?.Count ?? 0,
                    NumberOfFundingProjects = fundingProjects?.Count() ?? 0,
                    NumberOfMarketplaceProjects = marketplaceProjects?.Count() ?? 0,
                };

                return ResultDTO<object>.Success(response);
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

        public async Task<ResultDTO<object>> GetDashboardMilestones()
        {
            try
            {
                var milestoneOrders = await _unitOfWork.MilestoneRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .GroupBy(m => m.MilestoneOrder)
                    .Select(m => m.Key)
                    .ToListAsync();

                var response = new List<object>();

                foreach (var order in milestoneOrders)
                {
                    Expression<Func<ProjectMilestone, bool>> filter = p => p.Milestone.MilestoneOrder == order;

                    var projectMilestones = await _unitOfWork.ProjectMilestoneRepository
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(filter)
                        .ToListAsync();

                    response.Add(new
                    {
                        MilestoneOrder = order,
                        Count = projectMilestones.Count()
                    });
                }

                return ResultDTO<object>.Success(response);
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

        public async Task<ResultDTO<PaginatedResponse<TransactionInfoResponse>>> GetDashboardTransactions
            (ListRequest request, List<TransactionTypes>? transactionTypes)
        {
            try
            {
                //default filter
                Expression<Func<Transaction, bool>> filter = t => true;
                Expression<Func<Transaction, object>> orderBy = t => t.CreatedDate;

                // search value filter
                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    var searchFilter = (Expression<Func<Transaction, bool>>)(t =>
                        t.Description.ToLower().Contains(request.SearchValue.ToLower()));
                    filter = CombineFilters(filter, searchFilter);
                }

                // transaction type filter
                if (transactionTypes != null && transactionTypes.Any())
                {
                    var typeFilter = (Expression<Func<Transaction, bool>>)(t =>
                        transactionTypes.Contains(t.TransactionType)); // Match any type in the list
                    filter = CombineFilters(filter, typeFilter);
                }

                var list = await _unitOfWork.TransactionRepository.GetAllAsync(
                    filter: filter,
                    orderBy: orderBy,
                    isAscending: request.IsAscending.Value,
                    pageIndex: request.PageIndex,
                    pageSize: request.PageSize);

                var totalItems = _unitOfWork.TransactionRepository.GetAll(filter).Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);

                IEnumerable<TransactionInfoResponse> transactions = _mapper.Map<IEnumerable<TransactionInfoResponse>>(list);

                PaginatedResponse<TransactionInfoResponse> response = new PaginatedResponse<TransactionInfoResponse>
                {
                    PageSize = request.PageSize.Value,
                    PageIndex = request.PageIndex.Value,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = transactions
                };

                return ResultDTO<PaginatedResponse<TransactionInfoResponse>>.Success(response);
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

        public async Task<ResultDTO<object>> GetDashboardUsers()
        {
            try
            {
                var backers = await _userService.GetUsersByRoleAsync("Backer");
                var gameOwners = await _userService.GetUsersByRoleAsync("GameOwner");

                var response = new[]
                {
                    new { Role = "Backer", Count = backers?.Count ?? 0 },
                    new { Role = "GameOwner", Count = gameOwners?.Count ?? 0 }
                };

                return ResultDTO<object>.Success(response);
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

        public async Task<ResultDTO<decimal>> GetPlatformRevenue()
        {
            try
            {
                var systemWallet = await _unitOfWork.SystemWalletRepository.GetQueryable().SingleOrDefaultAsync();
                if (systemWallet == null)
                {
                    ResultDTO<decimal>.Success(0, "Platform balance");
                }
                var balance = (await _unitOfWork.SystemWalletRepository.GetQueryable().SingleOrDefaultAsync())?.TotalAmount ?? 0;
                return ResultDTO<decimal>.Success(balance, "Platform balance");
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

        public async Task<ResultDTO<SystemWallet>> GetSystemWallet()
        {
            try
            {
                var systemWallet = await _unitOfWork.SystemWalletRepository.GetQueryable().SingleOrDefaultAsync();
                return ResultDTO<SystemWallet>.Success(systemWallet);
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

        private Expression<Func<T, bool>> CombineFilters<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var combined = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter));
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }
    }
}
