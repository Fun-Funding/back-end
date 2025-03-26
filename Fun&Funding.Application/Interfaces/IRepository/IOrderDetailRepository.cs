using Fun_Funding.Domain.Entity;

namespace Fun_Funding.Application.IRepository
{
    public interface IOrderDetailRepository : IBaseRepository<OrderDetail>
    {
        Task<IEnumerable<OrderDetail>> GetOrderDetailsByMarketplaceProjectId(Guid marketplaceProjectId);
    }
}
