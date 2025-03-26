using AutoMapper;
using Fun_Funding.Application.Mapper.Resolver;
using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Application.ViewModel.CartDTO;
using Fun_Funding.Application.ViewModel.CategoryDTO;
using Fun_Funding.Application.ViewModel.ChatDTO;
using Fun_Funding.Application.ViewModel.CommissionDTO;
using Fun_Funding.Application.ViewModel.CouponDTO;
using Fun_Funding.Application.ViewModel.DigitalKeyDTO;
using Fun_Funding.Application.ViewModel.FeedbackDTO;
using Fun_Funding.Application.ViewModel.FundingFileDTO;
using Fun_Funding.Application.ViewModel.FundingProjectDTO;
using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;
using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
using Fun_Funding.Application.ViewModel.MilestoneDTO;
using Fun_Funding.Application.ViewModel.OrderDetailDTO;
using Fun_Funding.Application.ViewModel.OrderDTO;
using Fun_Funding.Application.ViewModel.PackageDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneBackerDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneRequirementDTO;
using Fun_Funding.Application.ViewModel.ProjectRequirementFileDTO;
using Fun_Funding.Application.ViewModel.RequirementDTO;
using Fun_Funding.Application.ViewModel.RewardItemDTO;
using Fun_Funding.Application.ViewModel.TransactionDTO;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Application.ViewModel.WalletDTO;
using Fun_Funding.Application.ViewModel.WithdrawDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;

namespace Fun_Funding.Application.Mapper
{
    public class MapperConfig : Profile
    {
        private readonly IUnitOfWork _unitOfWork;
        public MapperConfig(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public MapperConfig()
        {
            MappingFundingProject();
            MappingUser();
            MappingWallet();
            MappingCategory();
            MappingCommissionFee();
            MappingProjectMilestoneBacker();
            MappingProjectMilestone();
            MappingMilestone();
            MappingProjectMilestoneRequirement();
            MappingChat();
            MappingProjectCoupon();
            MappingMarketingProject();
            MappingMarketplaceFile();
            MappingOrder();
            MappingOrderDetail();
            MappingDigitalKey();
            MappingCart();
            MappingWithdraw();
            MappingFeedback();
        }
        public void MappingFundingProject()
        {
            CreateMap<FundingFileRequest, FundingFile>().ReverseMap();
            CreateMap<ItemAddRequest, RewardItem>().ReverseMap();
            CreateMap<PackageAddRequest, Package>()
                .ForMember(des => des.RewardItems, src => src.MapFrom(x => x.RewardItems)).ReverseMap();
            CreateMap<BankAccountRequest, BankAccount>().ReverseMap();
            CreateMap<BankAccountInfoResponse, BankAccount>().ReverseMap();
            CreateMap<PackageUpdateRequest, Package>().ReverseMap();
            CreateMap<ItemUpdateRequest, RewardItem>().ReverseMap();
            CreateMap<FundingFileResponse, FundingFile>().ReverseMap();
            CreateMap<FundingFileUpdateRequest, FundingFile>().ReverseMap();
            CreateMap<ItemResponse, RewardItem>().ReverseMap();
            CreateMap<PackageResponse, Package>().ReverseMap();
            CreateMap<FundingProjectAddRequest, FundingProject>()
                .ForMember(des => des.Packages, src => src.MapFrom(x => x.Packages))
                .ReverseMap();
            // UserInfoResponse -> User
            CreateMap<UserInfoResponse, User>()
                .ForPath(des => des.File.URL, src => src.MapFrom(x => x.Avatar))
                .ForMember(des => des.Wallet, src => src.MapFrom(x => x.Wallet))
                .ReverseMap();
            CreateMap<FundingProjectResponse, FundingProject>()
                .ForMember(des => des.User, src => src.MapFrom(x => x.User))
                .ForMember(des => des.SourceFiles, src => src.MapFrom(x => x.FundingFiles))
                .ForMember(des => des.Packages, src => src.MapFrom(x => x.Packages))
                .ForMember(des => des.Wallet, src => src.MapFrom(x => x.Wallet))
                .ReverseMap();
            CreateMap<BankAccountUpdateRequest, BankAccount>().ReverseMap();
            CreateMap<FundingProjectUpdateRequest, FundingProject>()
                .ForMember(des => des.Packages, src => src.MapFrom(x => x.Packages))
                .ReverseMap();
            //BankAccount Update
            CreateMap<BankAccount, BankAccountUpdateRequest>().ReverseMap();
        }

        public void MappingUser()
        {
            // UserUpdateRequest -> User
            CreateMap<UserUpdateRequest, User>()
                .ReverseMap();

            // UserInfoResponse -> User
            CreateMap<UserInfoResponse, User>()
                .ForPath(des => des.File.URL, src => src.MapFrom(x => x.Avatar))
                .ForMember(des => des.Wallet, src => src.MapFrom(x => x.Wallet))
                .ReverseMap();
        }

        public void MappingWallet()
        {
            //WalletInfoResponse -> Wallet
            CreateMap<WalletInfoResponse, Wallet>()
                .ForMember(des => des.Transactions, src => src.MapFrom(x => x.Transactions))
                .ForMember(des => des.WithdrawRequests, src => src.MapFrom(x => x.WithdrawRequests))
                .ReverseMap();
            CreateMap<TransactionInfoResponse, Fun_Funding.Domain.Entity.Transaction>()
                .ReverseMap();
            CreateMap<WithdrawResponse, WithdrawRequest>()
                .ReverseMap();

            CreateMap<WalletFundingResponse, Wallet>()
                .ForMember(des => des.BankAccount, src => src.MapFrom(x => x.BankAccount))
                .ReverseMap();
        }

        public void MappingCategory()
        {
            CreateMap<Category, CategoryResponse>().ReverseMap();
            CreateMap<CategoryRequest, Category>().ReverseMap();
            CreateMap<CategoryProjectRequest, Category>().ReverseMap();
        }

        public void MappingCommissionFee()
        {
            CreateMap<CommissionFee, CommissionFeeResponse>().ReverseMap();
            CreateMap<CommissionFeeAddRequest, CommissionFee>().ReverseMap();
            CreateMap<CommissionFeeUpdateRequest, CommissionFee>().ReverseMap();
        }

        public void MappingProjectMilestoneBacker()
        {
            CreateMap<ProjectMilestoneBacker, ProjectMilestoneBackerRequest>().ReverseMap();
            CreateMap<ProjectMilestoneBacker, ProjectMilestoneBackerResponse>()
                .ForMember(des => des.Backer, src => src.MapFrom(x => x.Backer))
                .ForMember(des => des.ProjectMilestone, src => src.MapFrom(x => x.ProjectMilestone))
                .ReverseMap();
        }

        public void MappingProjectMilestone()
        {
            CreateMap<ProjectMilestone, ProjectMilestoneRequest>().ReverseMap();
            CreateMap<ProjectMilestone, ProjectMilestoneResponse>()
                .ForMember(des => des.ProjectMilestoneRequirements, src => src.MapFrom(x => x.ProjectMilestoneRequirements))
                .ForMember(des => des.MilestoneName, src => src.MapFrom(x => x.Milestone.MilestoneName))
                .ForMember(des => des.Description, src => src.MapFrom(x => x.Milestone.Description))
                .ForMember(des => des.FundingProject, src => src.MapFrom(x => x.FundingProject))
                .ForMember(des => des.Milestone, src => src.MapFrom(x => x.Milestone))
                //.ForMember(
                //    des => des.LatestMilestone,
                //    src => src.MapFrom(x =>
                //        x.FundingProject.ProjectMilestones.OrderByDescending(m => m.Milestone.MilestoneOrder)
                //        .Select(pm => pm.Milestone)
                //        .FirstOrDefault()
                //    )
                //)
                .ForMember(
                    des => des.BackerAmount,
                    src => src.MapFrom(x =>
                        x.FundingProject.Packages
                            .SelectMany(p => p.PackageUsers)
                            .Count()
                    )
                )
                .ReverseMap();
            CreateMap<ProjectMilestoneRequirement, ProjectMilestoneRequirementResponse>()
                .ForMember(des => des.ReqDescription, src => src.MapFrom(x => x.Requirement.Description))
                .ForMember(des => des.RequirementTitle, src => src.MapFrom(x => x.Requirement.Title))
                .ReverseMap();
        }

        public void MappingMilestone()
        {
            CreateMap<Milestone, MilestoneResponse>().ReverseMap();
            CreateMap<Requirement, RequirementResponse>().ReverseMap();
        }

        public void MappingProjectMilestoneRequirement()
        {
            CreateMap<ProjectRequirementFile, ProjectRequirementFileResponse>().ReverseMap();
            CreateMap<ProjectRequirementFile, ProjectRequirementFileUpdateRequest>().ReverseMap();
            CreateMap<ProjectMilestoneRequirement, ProjectMilestoneRequirementUpdateRequest>().ReverseMap();
        }

        public void MappingOrder()
        {
            CreateMap<Order, OrderInfoResponse>()
                .ForMember(des => des.OrderDetails, src => src.MapFrom(x => x.OrderDetails))
                .ReverseMap();
        }

        public void MappingOrderDetail()
        {
            CreateMap<OrderDetail, OrderDetailInfoResponse>()
                .ForMember(des => des.DigitalKey, src => src.MapFrom(x => x.DigitalKey))
                .ReverseMap();
        }

        public void MappingDigitalKey()
        {
            CreateMap<DigitalKey, DigitalKeyInfoResponse>()
                .ForMember(des => des.MarketingProject, src => src.MapFrom(x => x.MarketplaceProject))
                .ReverseMap();
        }

        public void MappingMarketingProject()
        {
            CreateMap<MarketplaceProject, MarketplaceProjectOrderResponse>()
                .ReverseMap();
            CreateMap<MarketplaceProject, MarketplaceProjectInfoResponse>()
                .ForMember(dest => dest.MarketplaceFiles, opt => opt.MapFrom(src => src.MarketplaceFiles))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.FundingProject.Categories))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.FundingProject.User))
                .ForMember(dest => dest.Wallet, opt => opt.MapFrom(src => src.Wallet))
                .ReverseMap();
            CreateMap<MarketplaceProjectAddRequest, MarketplaceProject>()
                .ForMember(dest => dest.MarketplaceFiles, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<MarketplaceProjectUpdateRequest, MarketplaceProject>()
                .ForMember(dest => dest.MarketplaceFiles, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<MarketplaceProjectAddRequest, MarketplaceProjectUpdateRequest>()
                .ForMember(dest => dest.BankAccount, opt => opt.Ignore());
        }

        public void MappingMarketplaceFile()
        {
            CreateMap<MarketplaceFile, MarketplaceFileInfoResponse>().ReverseMap();
            CreateMap<MarketplaceFileRequest, MarketplaceFile>().ReverseMap();
            CreateMap<MarketplaceGameFileRequest, MarketplaceFile>().ReverseMap();
        }

        public void MappingChat()
        {
            CreateMap<Chat, ChatResponse>().AfterMap(async (src, des) =>
            {
                var sender = await _unitOfWork.UserRepository.GetByIdAsync(src.SenderId);
                var receiver = await _unitOfWork.UserRepository.GetByIdAsync(src.ReceiverId);

                // If sender or receiver are null, we'll use a default value or an empty string
                des.SenderName = sender?.FullName ?? "Unknown Sender";
                des.ReceiverName = receiver?.FullName ?? "Unknown Receiver";
            }).ReverseMap();

            CreateMap<Chat, ChatRequest>().ReverseMap();

        }
        public void MappingProjectCoupon()
        {
            CreateMap<ProjectCoupon, CouponResponse>()
                .ReverseMap();
        }
        public void MappingWithdraw()
        {
            CreateMap<WithdrawRequest, WithdrawResponse>()
                .ReverseMap();
        }

        public void MappingCart()
        {
            CreateMap<Cart, CartInfoResponse>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom<CartItemResolver>())
                .ReverseMap();
        }
        public void MappingFeedback()
        {
            CreateMap<Feedback, FeedbackRequest>().ReverseMap();
        }
    }
}
