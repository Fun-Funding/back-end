using Fun_Funding.Application;
using Fun_Funding.Application.ExternalServices.SoftDeleteService;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.Mapper;
using Fun_Funding.Application.Services.EntityServices;
using Fun_Funding.Application.Services.ExternalServices;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.ExternalServices.BackgroundWorkerService;
using Fun_Funding.Infrastructure.ExternalServices.StorageService;
using Fun_Funding.Infrastructure.Persistence.Database;
using Fun_Funding.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fun_Funding.Infrastructure.Dependency_Injection
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection service, IConfiguration configuration)
        {
            #region Db_Context
            //DBContext
            service.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(DIConfiguration).Assembly.FullName))
            .AddInterceptors(new SoftDeleteInterceptor()), ServiceLifetime.Scoped);
            // MongoDb
            service.AddScoped<MongoDBContext>();
            #endregion

            #region Identity
            //Identity
            service.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<MyDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Authenticaton
            //Authentication
            service.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(configuration["Jwt:key"]!))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };

            });
            #endregion

            #region Repositories
            service.AddScoped<ILikeRepository, LikeRepository>();
            service.AddScoped<IBankAccountRepository, BankAccountRepository>();
            service.AddScoped<ICategoryRepository, CategoryRepository>();
            service.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            service.AddScoped<IOrderRepository, OrderRepository>();
            service.AddScoped<IPackageBackerRepository, PackageBackerRepository>();
            service.AddScoped<IPackageRepository, PackageRepository>();
            service.AddScoped<IFundingProjectRepository, FundingProjectRepository>();
            service.AddScoped<IRewardItemRepository, RewardItemRepository>();
            service.AddScoped<ISourceFileRepository, SourceFileRepository>();
            service.AddScoped<ISystemWalletRepository, SystemWalletRepository>();
            service.AddScoped<ITransactionRepository, TransactionRepository>();
            service.AddScoped<IUserRepository, UserRepository>();
            service.AddScoped<IUserFileRepository, UserFileRepository>();
            service.AddScoped<IWalletRepository, WalletRepository>();
            service.AddScoped<IWithdrawRequestRepository, WithdrawRequestRepository>();
            service.AddScoped<ICommissionFeeRepository, CommissionFeeRepository>();
            service.AddScoped<ICommentRepository, CommentRepository>();
            service.AddScoped<IReportRepository, ReportRepository>();
            service.AddScoped<IFollowRepository, FollowRepository>();
            service.AddScoped<IProjectCouponRepository, ProjectCouponRepository>();
            service.AddScoped<IMilestoneRepository, MilestoneRepository>();
            service.AddScoped<IMarketplaceRepository, MarketplaceRepository>();
            service.AddScoped<IProjectMilestoneBackerRepository, ProjectMilestoneBackerRepository>();
            service.AddScoped<IProjectMilestoneRepository, ProjectMilestoneRepository>();
            service.AddScoped<IRequirementRepository, RequirementRepository>();
            service.AddScoped<IProjectMilestoneRequirementRepository, ProjectMilestoneRequirementRepository>();
            service.AddScoped<IProjectRequirementFileRepository, ProjectRequirementFileRepository>();
            service.AddScoped<ICreatorContractRepository, CreatorContractRepository>();
            service.AddScoped<IChatRepository, ChatRepository>();
            service.AddScoped<ICartRepository, CartRepository>();
            service.AddScoped<IMarketplaceFileRepository, MarketplaceFileRepository>();
            service.AddScoped<INotificationRepository, NotificationRepository>();
            service.AddScoped<IFeedbackRepository, FeedbackRepository>();
            #endregion

            #region Sevices
            service.AddScoped<IAuthenticationService, AuthenticationService>();
            service.AddScoped<ITokenGenerator, TokenGenerator>();
            service.AddScoped<IWithdrawService, WithdrawService>();
            service.AddScoped<IFundingProjectService, FundingProjectManagementService>();
            service.AddScoped<ICategoryService, CategoryService>();
            service.AddScoped<ICommissionFeeService, CommissionFeeService>();
            service.AddScoped<ILikeService, LikeService>();
            service.AddScoped<ICommentService, CommentService>();
            service.AddScoped<IProjectMilestoneBackerService, ProjectMilestoneBackerService>();
            service.AddScoped<IProjectMilestoneRequirementService, ProjectMilestoneRequirementService>();
            service.AddScoped<IProjectMilestoneService, ProjectMilestoneService>();
            service.AddScoped<IAzureService, AzureService>();
            service.AddScoped<ITransactionService, TransactionService>();
            service.AddScoped<IPackageBackerService, PackageBackerService>();
            service.AddScoped<IUserService, UserService>();
            service.AddScoped<IWalletService, WalletService>();
            service.AddScoped<IFollowService, FollowService>();
            service.AddScoped<IReportService, ReportService>();
            service.AddScoped<IMilestoneService, MilestoneService>();
            service.AddSingleton<IChatService, ChatService>();
            service.AddSingleton<IWebSocketManager, WebSocketManager>();
            service.AddScoped<IRequirementService, RequirementService>();
            service.AddScoped<IMarketplaceService, MarketplaceService>();
            service.AddScoped<IBackgroundProcessService, BackgroundProcessService>();
            service.AddScoped<IOrderService, OrderService>();
            service.AddScoped<ICreatorContractService, CreatorContractService>();
            service.AddScoped<IDigitalKeyService, DigitalKeyService>();
            service.AddScoped<IBankAccountService, BankAccountService>();
            service.AddScoped<IProjectCouponService, ProjectCouponService>();
            service.AddScoped<IBankAccountService, BankAccountService>();
            service.AddScoped<IFeedbackService, FeedbackService>();
            service.AddScoped<ICartService, CartService>();
            service.AddScoped<INotificationService, NotificationService>();
            service.AddScoped<IMarketplaceFileService, MarketplaceFileService>();
            service.AddScoped<ISystemWalletService, SystemWalletService>();
            #endregion



            //BaseRepository          
            service.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            service.AddScoped(typeof(IMongoBaseRepository<>), typeof(MongoBaseRepository<>));

            // Register the UnitOfWork
            service.AddScoped<IUnitOfWork, UnitOfWork>();
            service.AddAutoMapper(typeof(MapperConfig).Assembly);
            service.AddHostedService<WorkerService>();
            service.AddSignalR();
            return service;

        }
    }
}
