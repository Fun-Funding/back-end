using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class SystemWalletRepository : BaseRepository<SystemWallet>, ISystemWalletRepository
    {
        protected readonly MyDbContext _dbContext;
        public SystemWalletRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }
        public void UpdateSystemWallet(Guid id, SystemWallet systemWallet)
        {
            try
            {
                var r = _dbContext.SystemWallet.Find(id);
                if (r != null)
                {
                    _dbContext.Entry(systemWallet).State = EntityState.Detached;
                    _dbContext.SystemWallet.Attach(systemWallet);
                    _dbContext.Entry(systemWallet).State = EntityState.Modified;
                }
                else
                {
                    throw new Exception("Not Found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
