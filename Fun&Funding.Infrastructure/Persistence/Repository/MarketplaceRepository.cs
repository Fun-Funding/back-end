using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class MarketplaceRepository : BaseRepository<MarketplaceProject>, IMarketplaceRepository
    {
        private readonly MyDbContext _dbContext;
        public MarketplaceRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<MarketplaceProject?> GetMarketplaceProjectByFundingProjectId(Guid id)
        {
            var marketplaceProject = await _dbContext.MarketplaceProject
                    .Where(p => p.FundingProjectId == id)
                    .Include(p => p.Wallet)
                    .ThenInclude(p => p.BankAccount)
                    .FirstOrDefaultAsync();

            return marketplaceProject;
        }

        public async Task<MarketplaceProject?> GetMarketplaceProjectById(Guid id)
        {
            var marketplaceProject = await _dbContext.MarketplaceProject
                    .Where(p => p.Id == id)
                    .Include(p => p.MarketplaceFiles)
                    .Include(p => p.FundingProject.Categories)
                    .Include(p => p.FundingProject)
                    .ThenInclude(p => p.User)
                    .Include(p => p.Wallet)
                    .ThenInclude(p => p.BankAccount)
                    .FirstOrDefaultAsync();

            return marketplaceProject;
        }

        public async Task<MarketplaceProject?> GetNonDeletedMarketplaceProjectById(Guid id)
        {
            var marketplaceProject = await _dbContext.MarketplaceProject
                    .Where(p => p.Id == id && p.IsDeleted == false)
                    .Include(p => p.MarketplaceFiles)
                    .Include(p => p.FundingProject.Categories)
                    .Include(p => p.FundingProject)
                    .ThenInclude(p => p.User)
                    .Include(p => p.Wallet)
                    .ThenInclude(p => p.BankAccount)
                    .FirstOrDefaultAsync();

            return marketplaceProject;
        }
    }
}
