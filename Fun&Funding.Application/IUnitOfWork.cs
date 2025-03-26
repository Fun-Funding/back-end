using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Application.IRepository;

namespace Fun_Funding.Application
{
    public interface IUnitOfWork
    {
        // Repository interfaces
        ILikeRepository LikeRepository { get; }
        IFeedbackRepository FeedbackRepository { get; }
        ICommentRepository CommentRepository { get; }
        IFollowRepository FollowRepository { get; }
        IReportRepository ReportRepository { get; }
        IChatRepository ChatRepository { get; }
        IBankAccountRepository BankAccountRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IOrderDetailRepository OrderDetailRepository { get; }
        IOrderRepository OrderRepository { get; }
        IPackageBackerRepository PackageBackerRepository { get; }
        IPackageRepository PackageRepository { get; }
        IFundingProjectRepository FundingProjectRepository { get; }
        IRewardItemRepository RewardItemRepository { get; }
        ISourceFileRepository SourceFileRepository { get; }
        ISystemWalletRepository SystemWalletRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        IUserRepository UserRepository { get; }
        IUserFileRepository UserFileRepository { get; }
        IWalletRepository WalletRepository { get; }
        IWithdrawRequestRepository WithdrawRequestRepository { get; }
        ICommissionFeeRepository CommissionFeeRepository { get; }
        IProjectCouponRepository ProjectCouponRepository { get; }
        IMilestoneRepository MilestoneRepository { get; }
        IMarketplaceRepository MarketplaceRepository { get; }
        IProjectMilestoneBackerRepository ProjectMilestoneBackerRepository { get; }
        IProjectMilestoneRepository ProjectMilestoneRepository { get; }
        IRequirementRepository RequirementRepository { get; }
        IProjectMilestoneRequirementRepository ProjectMilestoneRequirementRepository { get; }
        IProjectRequirementFileRepository ProjectRequirementFileRepository { get; }
        ICreatorContractRepository CreatorContractRepository { get; }
        IDigitalKeyRepository DigitalKeyRepository { get; }
        ICartRepository CartRepository { get; }
        IMarketplaceFileRepository MarketplaceFileRepository { get; }
        INotificationRepository NotificationRepository { get; }

        // Methods for committing and rolling back
        void Commit();
        Task CommitAsync();
        void Rollback();
        Task RollbackAsync();
    }
}
