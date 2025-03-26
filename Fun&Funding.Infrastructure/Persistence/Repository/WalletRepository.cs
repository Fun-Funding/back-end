using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
    {
        protected readonly MyDbContext _dbContext;
        public WalletRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public void UpdateWallet(Guid id, Wallet wallet)
        {
            try
            {
                var r = _dbContext.Wallet.Find(id);
                if (r != null)
                {
                    _dbContext.Entry(wallet).State = EntityState.Detached;
                    _dbContext.Wallet.Attach(wallet);
                    _dbContext.Entry(wallet).State = EntityState.Modified;
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
