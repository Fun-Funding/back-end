using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CommissionDTO;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.IService
{
    public interface ICommissionFeeService
    {
        ResultDTO<CommissionFeeResponse> GetAppliedCommissionFee(CommissionType type);
        ResultDTO<List<CommissionFeeResponse>> GetListAppliedCommissionFee();
        Task<ResultDTO<CommissionFeeResponse>> UpdateCommsisionFee(Guid id, CommissionFeeUpdateRequest request);
        Task<ResultDTO<CommissionFeeResponse>> CreateCommissionFee(CommissionFeeAddRequest request);
        Task<ResultDTO<PaginatedResponse<CommissionFeeResponse>>> GetCommissionFees(ListRequest request, CommissionType? type);
    }
}
