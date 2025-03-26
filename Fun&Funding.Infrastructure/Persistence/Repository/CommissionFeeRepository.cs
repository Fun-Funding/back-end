using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Fun_Funding.Infrastructure.Persistence.Database;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class CommissionFeeRepository : BaseRepository<CommissionFee>, ICommissionFeeRepository
    {
        private readonly MyDbContext _dbContext;
        public CommissionFeeRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public CommissionFee? GetAppliedCommissionFeeByType(CommissionType commissionType)
        {
            var commissionFee = _dbContext.CommissionFee.Where(c => c.CommissionType == commissionType)
                                .OrderByDescending(c => c.UpdateDate)
                                .FirstOrDefault();

            return commissionFee;
        }
    }
}
