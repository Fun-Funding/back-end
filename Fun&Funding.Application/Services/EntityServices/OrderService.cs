using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.OrderDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;
        private readonly IDigitalKeyService _digitalKeyService;
        private readonly ICommissionFeeService _commissionFeeService;
        private readonly INotificationService _notificationService;
        public OrderService(IUnitOfWork unitOfWork, UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper, IDigitalKeyService digitalKeyService, ICommissionFeeService commissionFeeService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _roleManager = roleManager;
            _mapper = mapper;
            _digitalKeyService = digitalKeyService;
            _commissionFeeService = commissionFeeService;
            _notificationService = notificationService;
        }

        public async Task<ResultDTO<Guid>> CreateOrder(CreateOrderRequest createOrderRequest)
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

                var wallet = await _unitOfWork.WalletRepository.GetAsync(w => w.Backer.Id == user.Id);
                if (wallet == null)
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Wallet Not Found.");

                decimal totalCost = 0;
                if (createOrderRequest.CartItems.Count <= 0)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "No Items in Cart");
                }
                foreach (var cartItem in createOrderRequest.CartItems)
                {
                    var marketplaceProject = _unitOfWork.MarketplaceRepository.GetById(cartItem.MarketplaceProjectId);
                    if (marketplaceProject == null)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Marketplace project not found.");
                    }
                    if (marketplaceProject.Status != ProjectStatus.Processing)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Game is not allowed to purchased");
                    }
                    if (cartItem.Price < 0)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Invalid game price");
                    }
                    var gamePrice = cartItem.Price;
                    // Check if Marketplace game has a coupon
                    if (cartItem.AppliedCoupon != null)
                    {
                        ProjectCoupon projectCoupon = _unitOfWork.ProjectCouponRepository.GetById(cartItem.AppliedCoupon.Id);
                        if (projectCoupon == null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.NotFound, "Project Coupon Not Found.");
                        }
                        else if (projectCoupon.Status == ProjectCouponStatus.Disable)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "Coupon Already Used.");
                        }
                        gamePrice = gamePrice * (1 - cartItem.AppliedCoupon.DiscountRate);
                    }
                    totalCost += gamePrice;
                }

                // Check Wallet Balance
                if (wallet.Balance < totalCost)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Insufficient Wallet Balance.");
                }

                // Remove Amount Order Money From Wallet
                wallet.Balance -= totalCost;

                // Update Backer's Wallet
                _unitOfWork.WalletRepository.UpdateWallet(wallet.Id, wallet);
                // Create new Order
                Order order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TotalPrice = totalCost,
                    CreatedDate = DateTime.Now,
                    OrderDetails = new List<OrderDetail>()
                };

                // Create Backer's Transaction
                Transaction purchaseTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    TotalAmount = -totalCost,
                    TransactionType = TransactionTypes.OrderPurchase,
                    Description = $"Purchase Order at {DateTime.Now}",
                    WalletId = wallet.Id,
                    OrderId = order.Id,
                    CreatedDate = DateTime.Now,
                };

                foreach (var cartItem in createOrderRequest.CartItems)
                {
                    var commissionFee = _commissionFeeService.GetAppliedCommissionFee(CommissionType.MarketplaceCommission)._data;
                    var gameWallet = await _unitOfWork.WalletRepository.GetAsync(w => w.MarketplaceProject.Id == cartItem.MarketplaceProjectId);

                    // Create digital key for Marketplace Project
                    DigitalKey digitalKey = new DigitalKey
                    {
                        Id = Guid.NewGuid(),
                        KeyString = _digitalKeyService.GenerateGameKey(),
                        Status = KeyStatus.ACTIVE,
                        CreatedDate = DateTime.Now,
                        MarketplaceProject = _unitOfWork.MarketplaceRepository.GetById(cartItem.MarketplaceProjectId)
                    };

                    if (gameWallet == null)
                    {
                        throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace Game Wallet Not Found.");
                    }
                    else
                    {
                        var receivedMoney = cartItem.Price;
                        decimal systemReceivedMoney = 0;

                        // Create OrderDetail
                        OrderDetail orderDetail = new OrderDetail();
                        // Create Game Owner Transaction
                        Transaction recieveTransaction = new Transaction();
                        // Create System Transaction
                        Transaction systemTransaction = new Transaction();
                        // Get SystemWallet
                        var systemWallet = await _unitOfWork.SystemWalletRepository.GetAsync(s => true);
                        // Check if have coupon
                        if (cartItem.AppliedCoupon != null)
                        {
                            ProjectCoupon projectCoupon = _unitOfWork.ProjectCouponRepository.GetById(cartItem.AppliedCoupon.Id);
                            if (projectCoupon == null)
                            {
                                throw new ExceptionError((int)HttpStatusCode.NotFound, "Project Coupon Not Found.");
                            }
                            else if (projectCoupon.Status == ProjectCouponStatus.Disable)
                            {
                                throw new ExceptionError((int)HttpStatusCode.BadRequest, "Coupon Already Used Up.");
                            }
                            projectCoupon.Status = ProjectCouponStatus.Disable;
                            // Money left when applying coupon
                            receivedMoney = receivedMoney * (1 - cartItem.AppliedCoupon.DiscountRate);

                            // Extra fee on commission rate
                            systemReceivedMoney = receivedMoney * commissionFee.Rate;
                            receivedMoney -= systemReceivedMoney;

                            // Add OrderDetail
                            orderDetail = new OrderDetail
                            {
                                Id = Guid.NewGuid(),
                                DigitalKey = digitalKey,
                                OrderId = order.Id,
                                UnitPrice = cartItem.Price * (1 - cartItem.AppliedCoupon.DiscountRate),
                                CreatedDate = DateTime.Now,
                                ProjectCouponId = cartItem.AppliedCoupon?.Id
                            };

                            // Add Marketplace Transaction
                            recieveTransaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                TotalAmount = receivedMoney,
                                TransactionType = TransactionTypes.OrderPurchase,
                                Wallet = gameWallet,
                                Description = $"Receive money from game {_unitOfWork.MarketplaceRepository.GetById(cartItem.MarketplaceProjectId).Name} purchase",
                                OrderDetailId = orderDetail.Id,
                                CommissionFee = await _unitOfWork.CommissionFeeRepository.GetByIdAsync(commissionFee.Id),
                                CreatedDate = DateTime.Now,
                            };
                            // Add System Transaction
                            systemTransaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                TotalAmount = systemReceivedMoney,
                                TransactionType = TransactionTypes.CommissionFee,
                                Description = $"Receive commission money from game {_unitOfWork.MarketplaceRepository.GetById(cartItem.MarketplaceProjectId).Name} purchase",
                                SystemWallet = systemWallet,
                                WalletId = gameWallet.Id,
                                OrderDetailId = orderDetail.Id,
                                CommissionFee = await _unitOfWork.CommissionFeeRepository.GetByIdAsync(commissionFee.Id),
                                CreatedDate = DateTime.Now,
                            };

                            // Add Order's OrderDetail
                            order.OrderDetails.Add(orderDetail);
                            // Update Project Coupon
                            _unitOfWork.ProjectCouponRepository.Update(projectCoupon);
                        }
                        else
                        {
                            // Extra fee on commission rate
                            systemReceivedMoney = receivedMoney * commissionFee.Rate;
                            receivedMoney -= systemReceivedMoney;

                            // Add OrderDetail
                            orderDetail = new OrderDetail
                            {
                                Id = Guid.NewGuid(),
                                DigitalKey = digitalKey,
                                OrderId = order.Id,
                                UnitPrice = cartItem.Price,
                                CreatedDate = DateTime.Now,
                            };

                            // Add Marketplace Transaction
                            recieveTransaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                TotalAmount = receivedMoney,
                                TransactionType = TransactionTypes.OrderPurchase,
                                Wallet = gameWallet,
                                Description = $"Receive money from game {_unitOfWork.MarketplaceRepository.GetById(cartItem.MarketplaceProjectId).Name} purchase",
                                OrderDetailId = orderDetail.Id,
                                CommissionFee = await _unitOfWork.CommissionFeeRepository.GetByIdAsync(commissionFee.Id),
                                CreatedDate = DateTime.Now,
                            };

                            // Add System Transaction
                            systemTransaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                TotalAmount = systemReceivedMoney,
                                TransactionType = TransactionTypes.CommissionFee,
                                SystemWallet = systemWallet,
                                WalletId = gameWallet.Id,
                                Description = $"Receive commission money from game {_unitOfWork.MarketplaceRepository.GetById(cartItem.MarketplaceProjectId).Name} purchase",
                                OrderDetailId = orderDetail.Id,
                                CommissionFee = await _unitOfWork.CommissionFeeRepository.GetByIdAsync(commissionFee.Id),
                                CreatedDate = DateTime.Now,
                            };

                            // Add Order's OrderDetail
                            order.OrderDetails.Add(orderDetail);
                        }
                        // Update Marketplace Wallet
                        gameWallet.Balance += receivedMoney;
                        // Update SystemWallet
                        systemWallet.TotalAmount += systemReceivedMoney;

                        // Add DigitalKey
                        _unitOfWork.DigitalKeyRepository.Add(digitalKey);
                        // Save Game Owner Transaction
                        _unitOfWork.TransactionRepository.Add(recieveTransaction);
                        // Save System Transaction
                        _unitOfWork.TransactionRepository.Add(systemTransaction);
                        // Update GameOwner Wallet
                        _unitOfWork.WalletRepository.UpdateWallet(gameWallet.Id, gameWallet);
                        // Save SystemWallet
                        _unitOfWork.SystemWalletRepository.UpdateSystemWallet(systemWallet.Id, systemWallet);
                    }
                }
                // Add Backer's Order
                _unitOfWork.OrderRepository.Add(order);
                // Add Backer's Transaction
                _unitOfWork.TransactionRepository.Add(purchaseTransaction);
                await _unitOfWork.CommitAsync();

                // NOTIFICATION
                List<Guid> recipientsId = new List<Guid>();
                foreach (var cartItem in createOrderRequest.CartItems)
                {
                    // 1. get recipientsIds
                    var marketplaceProject = await _unitOfWork.MarketplaceRepository
                        .GetQueryable()
                        .Include(m => m.FundingProject)
                        .FirstOrDefaultAsync(m => m.Id == cartItem.MarketplaceProjectId);
                    if (marketplaceProject != null)
                    {
                        recipientsId.Add(marketplaceProject.FundingProject.UserId);
                        // 2. initiate new Notification object
                        var notification = new Notification
                        {
                            Id = new Guid(),
                            Date = DateTime.Now,
                            Message = $"purchased game <b>{marketplaceProject.Name}</b>",
                            NotificationType = NotificationType.MarketplacePurchase,
                            Actor = new { user.Id, user.UserName, user.File?.URL },
                            ObjectId = marketplaceProject.Id,
                        };
                        // 3. send noti
                        await _notificationService.SendNotification(notification, recipientsId);
                    }

                }



                return ResultDTO<Guid>.Success(order.Id, "Order added successfully!");
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

        public async Task<ResultDTO<PaginatedResponse<OrderInfoResponse>>> GetAllOrders(ListRequest request)
        {
            try
            {
                Expression<Func<Order, bool>> filter = null;
                Expression<Func<Order, object>> orderBy = u => u.CreatedDate;

                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "amount":
                            orderBy = u => u.TotalPrice;
                            break;
                        default:
                            orderBy = u => u.CreatedDate;
                            break;
                    }
                }

                if (request.From != null && request.To != null)
                {
                    DateTime fromDate = (DateTime)request.From;
                    DateTime toDate = (DateTime)request.To;
                    filter = c => c.CreatedDate >= fromDate && c.CreatedDate <= toDate;
                }
                else if (request.From != null)
                {
                    DateTime fromDate = (DateTime)request.From;
                    filter = c => c.CreatedDate >= fromDate;
                }
                else if (request.To != null)
                {
                    DateTime toDate = (DateTime)request.To;
                    filter = c => c.CreatedDate <= toDate;
                }

                var list = await _unitOfWork.OrderRepository.GetAllAsync(
                       filter: filter,
                       orderBy: orderBy,
                       isAscending: request.IsAscending.Value,
                       pageIndex: request.PageIndex,
                       pageSize: request.PageSize,
                       includeProperties: "OrderDetails,OrderDetails.DigitalKey,OrderDetails.DigitalKey.MarketplaceProject,OrderDetails.ProjectCoupon,OrderDetails.DigitalKey.MarketplaceProject.MarketplaceFiles");

                var totalItems = _unitOfWork.OrderRepository.GetAll(filter).Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                IEnumerable<OrderInfoResponse> orders = _mapper.Map<IEnumerable<OrderInfoResponse>>(list);

                PaginatedResponse<OrderInfoResponse> response = new PaginatedResponse<OrderInfoResponse>
                {
                    PageSize = request.PageSize.Value,
                    PageIndex = request.PageIndex.Value,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = orders
                };

                return ResultDTO<PaginatedResponse<OrderInfoResponse>>.Success(response, "Orders found!");

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

        public async Task<ResultDTO<OrderInfoResponse>> GetOrderById(Guid orderId)
        {
            try
            {
                Order? order = await _unitOfWork.OrderRepository.GetQueryable()
                    .AsNoTracking()
                    .Where(o => o.Id == orderId)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.DigitalKey)
                            .ThenInclude(dk => dk.MarketplaceProject)
                                .ThenInclude(dk => dk.MarketplaceFiles)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.ProjectCoupon)
                    .SingleOrDefaultAsync();

                if (order == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Order Not Found.");
                }
                else
                {
                    var orderResponse = _mapper.Map<OrderInfoResponse>(order);
                    return ResultDTO<OrderInfoResponse>.Success(orderResponse, "Order found!");
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

        public async Task<ResultDTO<IEnumerable<object>>> GetOrderDetailsByMarketplaceProjectId(Guid marketplaceProjectId)
        {
            try
            {
                var orderDetails = await _unitOfWork.OrderDetailRepository
                    .GetOrderDetailsByMarketplaceProjectId(marketplaceProjectId);

                var response = orderDetails.Select(
                    o => new
                    {
                        o.Id,
                        o.UnitPrice,
                        KeyString = o.DigitalKey.KeyString,
                        o.CreatedDate,
                        CouponKey = o.ProjectCoupon?.CouponKey ?? string.Empty,
                        CouponName = o.ProjectCoupon?.CouponName ?? string.Empty,
                        DiscountRate = o.ProjectCoupon?.DiscountRate ?? 0,
                        MarketplaceProjectId = o.DigitalKey.MarketplaceProject.Id,
                        OrderId = o.OrderId,
                        DigitalKeyStatus = o.DigitalKey.Status,
                        UserId = o.Order.UserId,
                        Name = o.Order.User?.UserName,
                        Email = o.Order.User?.Email,
                        Avatar = o.Order.User?.File?.URL ?? string.Empty
                    }).ToList();

                return ResultDTO<IEnumerable<object>>.Success(response);
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

        public async Task<ResultDTO<List<OrderSummary>>> GetOrdersGroupedByDate(Guid marketplaceProjectId)
        {
            try
            {
                var ordersGroupedByDate = await _unitOfWork.OrderRepository.GetQueryable()
                    .Include(o => o.OrderDetails).ThenInclude(od => od.DigitalKey)
                .Where(o => o.OrderDetails.Any(od => od.DigitalKey.MarketplaceProject.Id == marketplaceProjectId))
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new OrderSummary
                {
                    CreatedDate = g.Key,
                    TotalAmount = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(summary => summary.CreatedDate)
                .ToListAsync();

                return ResultDTO<List<OrderSummary>>.Success(ordersGroupedByDate);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<ResultDTO<PaginatedResponse<OrderInfoResponse>>> GetUserOrders(ListRequest request)
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
                Expression<Func<Order, bool>> filter = o => o.UserId == user.Id;
                Expression<Func<Order, object>> orderBy = u => u.CreatedDate;

                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "amount":
                            orderBy = u => u.TotalPrice;
                            break;
                        default:
                            orderBy = u => u.CreatedDate;
                            break;
                    }
                }

                if (request.From != null && request.To != null)
                {
                    DateTime fromDate = (DateTime)request.From;
                    DateTime toDate = (DateTime)request.To;
                    filter = c => c.CreatedDate >= fromDate && c.CreatedDate <= toDate && c.UserId == user.Id;
                }
                else if (request.From != null)
                {
                    DateTime fromDate = (DateTime)request.From;
                    filter = c => c.CreatedDate >= fromDate && c.UserId == user.Id;
                }
                else if (request.To != null)
                {
                    DateTime toDate = (DateTime)request.To;
                    filter = c => c.CreatedDate <= toDate && c.UserId == user.Id;
                }

                var list = await _unitOfWork.OrderRepository.GetAllAsync(
                       filter: filter,
                       orderBy: orderBy,
                       isAscending: request.IsAscending.Value,
                       pageIndex: request.PageIndex,
                       pageSize: request.PageSize,
                       includeProperties: "OrderDetails,OrderDetails.DigitalKey,OrderDetails.DigitalKey.MarketplaceProject,OrderDetails.ProjectCoupon,OrderDetails.DigitalKey.MarketplaceProject.MarketplaceFiles");

                    var totalItems = _unitOfWork.OrderRepository.GetAll(filter).Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                    IEnumerable<OrderInfoResponse> orders = _mapper.Map<IEnumerable<OrderInfoResponse>>(list);

                    PaginatedResponse<OrderInfoResponse> response = new PaginatedResponse<OrderInfoResponse>
                    {
                        PageSize = request.PageSize.Value,
                        PageIndex = request.PageIndex.Value,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = orders
                    };

                    return ResultDTO<PaginatedResponse<OrderInfoResponse>>.Success(response, "Orders found!");
                

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
