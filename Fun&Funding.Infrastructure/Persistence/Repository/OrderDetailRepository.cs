using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class OrderDetailRepository : BaseRepository<OrderDetail>, IOrderDetailRepository
    {
        private readonly MyDbContext _dbContext;
        public OrderDetailRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByMarketplaceProjectId(Guid marketplaceProjectId)
        {
            try
            {
                var orderDetails = await _dbContext.OrderDetail
                    .AsNoTracking()
                    .Include(o => o.Order)
                    .ThenInclude(o => o.User)
                    .ThenInclude(u => u.File)
                    .Include(o => o.DigitalKey)
                    .ThenInclude(o => o.MarketplaceProject)
                    .Include(o => o.ProjectCoupon)
                    .Where(o => o.DigitalKey.MarketplaceProject.Id == marketplaceProjectId)
                    .OrderByDescending(o => o.CreatedDate)
                    .ToListAsync();

                return orderDetails;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
