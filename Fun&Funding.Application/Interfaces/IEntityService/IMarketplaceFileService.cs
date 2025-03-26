using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;

namespace Fun_Funding.Application.Interfaces.IEntityService
{
    public interface IMarketplaceFileService
    {
        Task<ResultDTO<MarketplaceFileInfoResponse>> UploadGameUpdateFile
            (Guid marketplaceProjectId, MarketplaceGameFileRequest request);

        Task<ResultDTO<PaginatedResponse<object>>> GetGameFiles
            (Guid marketplaceProjectId, ListRequest request);
    }
}
