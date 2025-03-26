using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;

namespace Fun_Funding.Application.Interfaces.IRepository
{
    public interface IMarketplaceFileRepository : IBaseRepository<MarketplaceFile>
    {
        void DeleteMarketplaceFile(MarketplaceFile marketplaceFile);
    }
}
