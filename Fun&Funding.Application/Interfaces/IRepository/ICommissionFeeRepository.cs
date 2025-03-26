using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.IRepository
{
    public interface ICommissionFeeRepository : IBaseRepository<CommissionFee>
    {
        CommissionFee? GetAppliedCommissionFeeByType(CommissionType commissionType);
    }
}
