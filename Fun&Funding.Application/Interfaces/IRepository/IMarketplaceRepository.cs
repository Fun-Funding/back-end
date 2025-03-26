using Fun_Funding.Domain.Entity;

namespace Fun_Funding.Application.IRepository
{
    public interface IMarketplaceRepository : IBaseRepository<MarketplaceProject>
    {
        Task<MarketplaceProject?> GetMarketplaceProjectByFundingProjectId(Guid id);
        Task<MarketplaceProject?> GetNonDeletedMarketplaceProjectById(Guid id);
        Task<MarketplaceProject?> GetMarketplaceProjectById(Guid id);
    }
}
