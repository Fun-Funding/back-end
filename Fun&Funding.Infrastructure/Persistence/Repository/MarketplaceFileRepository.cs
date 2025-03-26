using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class MarketplaceFileRepository : BaseRepository<MarketplaceFile>, IMarketplaceFileRepository
    {
        private readonly MyDbContext _context;
        public MarketplaceFileRepository(MyDbContext context) : base(context)
        {
            _context = context;
        }

        public void DeleteMarketplaceFile(MarketplaceFile marketplaceFile)
        {
            var trackedFile = _context.MarketplaceFile.Local
                    .FirstOrDefault(f => f.Id == marketplaceFile.Id);

            if (trackedFile == null)
            {
                _context.MarketplaceFile.Attach(marketplaceFile);  // Attach only if not already tracked
                trackedFile = marketplaceFile;
            }

            // Soft delete logic
            trackedFile.IsDeleted = true;
            trackedFile.DeletedAt = DateTimeOffset.Now;

            _context.Entry(trackedFile).State = EntityState.Modified;
        }

        //public async Task HardDeleteMarketplaceFilesAsync(IEnumerable<MarketplaceFile> files)
        //{
        //    if (files == null || files.Count() == 0)
        //        return;

        //    // Extract the IDs of the files to be deleted
        //    var ids = files.Select(f => f.Id).ToList();

        //    // Create the raw SQL command
        //    var sqlCommand = $"DELETE FROM MarketplaceFiles WHERE Id IN ({string.Join(",", ids.Select(id => $"'{id}'"))})";

        //    // Execute the raw SQL command asynchronously
        //    await _context.Database.ExecuteSqlRawAsync(sqlCommand);
        //}
    }
}
