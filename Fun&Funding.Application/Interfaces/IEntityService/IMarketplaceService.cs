using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.IService
{
    public interface IMarketplaceService
    {
        Task<ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>> GetAllMarketplaceProject
            (ListRequest request, List<Guid>? categoryIds, List<ProjectStatus>? statusList, decimal? fromPrice, decimal? toPrice);
        Task<ResultDTO<MarketplaceProjectInfoResponse>> CreateMarketplaceProject(MarketplaceProjectAddRequest request);
        Task<ResultDTO<MarketplaceProjectInfoResponse>> GetMarketplaceProjectById(Guid id);
        Task DeleteMarketplaceProject(Guid id);
        Task<ResultDTO<MarketplaceProjectInfoResponse>>
            UpdateMarketplaceProject(Guid id, MarketplaceProjectUpdateRequest request, bool? isDeleted = null);
        Task<ResultDTO<MarketplaceProjectInfoResponse>>
            UpdateMarketplaceProjectStatus(Guid id, ProjectStatus status, string? note);
        Task<ResultDTO<List<MarketplaceProjectInfoResponse>>> GetTop3MostPurchasedOngoingMarketplaceProject();
        Task<ResultDTO<List<MarketplaceProjectInfoResponse>>> GetTop4MostPurchasedOngoingMarketplaceProject();
        Task<ResultDTO<decimal>> CountPlatformProjects();
        Task<ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>> GetBackerPurchasedMarketplaceProject(ListRequest request);
        Task<ResultDTO<PaginatedResponse<MarketplaceProjectInfoResponse>>> GetGameOwnerMarketplaceProject(ListRequest request);
    }
}
