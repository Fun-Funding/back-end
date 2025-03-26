using Fun_Funding.Application;
using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Infrastructure.Persistence.Database;
using Fun_Funding.Infrastructure.Persistence.Repository;

namespace Fun_Funding.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _dbContext;
        private readonly MongoDBContext _mongoDBContext;

        // Repositories
        private ILikeRepository _likeRepository;
        private IFeedbackRepository _feedbackRepository;
        private ICommentRepository _commentRepository;
        private IFollowRepository _followRepository;
        private IReportRepository _reportRepository;
        private IChatRepository _chatRepository;
        private IBankAccountRepository _bankAccountRepository;
        private ICategoryRepository _categoryRepository;
        private IOrderDetailRepository _orderDetailRepository;
        private IOrderRepository _orderRepository;
        private IPackageBackerRepository _packageBackerRepository;
        private IPackageRepository _packageRepository;
        private IFundingProjectRepository _fundingProjectRepository;
        private IRewardItemRepository _rewardItemRepository;
        private ISourceFileRepository _sourceFileRepository;
        private ISystemWalletRepository _systemWalletRepository;
        private ITransactionRepository _transactionRepository;
        private IUserRepository _userRepository;
        private IUserFileRepository _userFileRepository;
        private IWalletRepository _walletRepository;
        private IWithdrawRequestRepository _withdrawRequestRepository;
        private ICommissionFeeRepository _commissionFeeRepository;
        private IMarketplaceRepository _marketplaceRepository;
        private IProjectCouponRepository _projectCouponRepository;
        private IMilestoneRepository _milestoneRepository;
        private IProjectMilestoneBackerRepository _projectMilestoneBackerRepository;
        private IProjectMilestoneRepository _projectMilestoneRepository;
        private IRequirementRepository _requirementRepository;
        private IProjectMilestoneRequirementRepository _projectMilestoneRequirementRepository;
        private IProjectRequirementFileRepository _projectRequirementFileRepository;
        private ICreatorContractRepository _creatorContractRepository;
        private IDigitalKeyRepository _digitalKeyRepository;
        private ICartRepository _cartRepository;
        private INotificationRepository _notificationRepository;
        private IMarketplaceFileRepository _marketplaceFileRepository;
        public UnitOfWork(MyDbContext dbContext, MongoDBContext mongoDBContext)
        {
            _dbContext = dbContext;
            _mongoDBContext = mongoDBContext;
        }

        // Repository properties
        public IBankAccountRepository BankAccountRepository
        {
            get
            {
                return _bankAccountRepository = _bankAccountRepository ?? new BankAccountRepository(_dbContext);
            }
        }

        public ICategoryRepository CategoryRepository
        {
            get
            {
                return _categoryRepository = _categoryRepository ?? new CategoryRepository(_dbContext);
            }
        }

        public IOrderDetailRepository OrderDetailRepository
        {
            get
            {
                return _orderDetailRepository = _orderDetailRepository ?? new OrderDetailRepository(_dbContext);
            }
        }

        public IOrderRepository OrderRepository
        {
            get
            {
                return _orderRepository = _orderRepository ?? new OrderRepository(_dbContext);
            }
        }

        public IPackageBackerRepository PackageBackerRepository
        {
            get
            {
                return _packageBackerRepository = _packageBackerRepository ?? new PackageBackerRepository(_dbContext);
            }
        }

        public IPackageRepository PackageRepository
        {
            get
            {
                return _packageRepository = _packageRepository ?? new PackageRepository(_dbContext);
            }
        }

        public IFundingProjectRepository FundingProjectRepository
        {
            get
            {
                return _fundingProjectRepository = _fundingProjectRepository ?? new FundingProjectRepository(_dbContext);
            }
        }

        public IRewardItemRepository RewardItemRepository
        {
            get
            {
                return _rewardItemRepository = _rewardItemRepository ?? new RewardItemRepository(_dbContext);
            }
        }

        public ISourceFileRepository SourceFileRepository
        {
            get
            {
                return _sourceFileRepository = _sourceFileRepository ?? new SourceFileRepository(_dbContext);
            }
        }

        public ISystemWalletRepository SystemWalletRepository
        {
            get
            {
                return _systemWalletRepository = _systemWalletRepository ?? new SystemWalletRepository(_dbContext);
            }
        }

        public ITransactionRepository TransactionRepository
        {
            get
            {
                return _transactionRepository = _transactionRepository ?? new TransactionRepository(_dbContext);
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                return _userRepository = _userRepository ?? new UserRepository(_dbContext);
            }
        }

        public IUserFileRepository UserFileRepository
        {
            get
            {
                return _userFileRepository = _userFileRepository ?? new UserFileRepository(_dbContext);
            }
        }

        public IWalletRepository WalletRepository
        {
            get
            {
                return _walletRepository = _walletRepository ?? new WalletRepository(_dbContext);
            }
        }

        public IWithdrawRequestRepository WithdrawRequestRepository
        {
            get
            {
                return _withdrawRequestRepository = _withdrawRequestRepository ?? new WithdrawRequestRepository(_dbContext);
            }
        }

        public ICommissionFeeRepository CommissionFeeRepository
        {
            get
            {
                return _commissionFeeRepository = _commissionFeeRepository ?? new CommissionFeeRepository(_dbContext);
            }
        }

        public ILikeRepository LikeRepository
        {
            get
            {
                return _likeRepository = _likeRepository ?? new LikeRepository(_mongoDBContext);
            }
        }

        public ICommentRepository CommentRepository
        {
            get
            {
                return _commentRepository = _commentRepository ?? new CommentRepository(_mongoDBContext);
            }
        }

        public IDigitalKeyRepository DigitalKeyRepository
        {
            get
            {
                return _digitalKeyRepository = _digitalKeyRepository ?? new DigitalKeyRepository(_dbContext);
            }
        }

        public IProjectCouponRepository ProjectCouponRepository =>
       _projectCouponRepository ??= new ProjectCouponRepository(_dbContext);

        public IMilestoneRepository MilestoneRepository =>
            _milestoneRepository ??= new MilestoneRepository(_dbContext);

        public IProjectMilestoneRepository ProjectMilestoneRepository =>
            _projectMilestoneRepository ??= new ProjectMilestoneRepository(_dbContext);

        public IRequirementRepository RequirementRepository =>
            _requirementRepository ??= new RequirementRepository(_dbContext);

        public IProjectMilestoneRequirementRepository ProjectMilestoneRequirementRepository =>
            _projectMilestoneRequirementRepository ??= new ProjectMilestoneRequirementRepository(_dbContext);

        public IProjectRequirementFileRepository ProjectRequirementFileRepository =>
            _projectRequirementFileRepository ??= new ProjectRequirementFileRepository(_dbContext);

        public IProjectMilestoneBackerRepository ProjectMilestoneBackerRepository =>
            _projectMilestoneBackerRepository ??= new ProjectMilestoneBackerRepository(_dbContext);

        public IFollowRepository FollowRepository =>
            _followRepository ??= new FollowRepository(_mongoDBContext);

        public IReportRepository ReportRepository =>
            _reportRepository ??= new ReportRepository(_mongoDBContext);

        public IChatRepository ChatRepository =>
            _chatRepository ??= new ChatRepository(_mongoDBContext);

        public IMarketplaceRepository MarketplaceRepository =>
            _marketplaceRepository ??= new MarketplaceRepository(_dbContext);

        public ICreatorContractRepository CreatorContractRepository =>
            _creatorContractRepository ??= new CreatorContractRepository(_mongoDBContext);

        public ICartRepository CartRepository =>
            _cartRepository ??= new CartRepository(_mongoDBContext);

        public IMarketplaceFileRepository MarketplaceFileRepository =>
            _marketplaceFileRepository ??= new MarketplaceFileRepository(_dbContext);

        public INotificationRepository NotificationRepository =>
            _notificationRepository ??= new NotificationRepository(_mongoDBContext);

        public IFeedbackRepository FeedbackRepository => 
            _feedbackRepository ??= new FeedbackRepository(_mongoDBContext);

        // Commit and rollback methods
        public void Commit()
             => _dbContext.SaveChanges();

        public async Task CommitAsync()
            => await _dbContext.SaveChangesAsync();

        public void Rollback()
            => _dbContext.Dispose();

        public async Task RollbackAsync()
            => await _dbContext.DisposeAsync();
    }

}
