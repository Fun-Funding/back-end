using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.TransactionDTO;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface ITransactionService
    {
        Task CreateTransactionAsync(
            decimal totalAmount,
            string description,
            TransactionTypes transactionType,
            Guid walletId,
            Guid? packageId = null,
            Guid? systemWalletId = null,
            Guid? commissionFeeId = null,
            Guid? orderId = null,
            Guid? orderDetailId = null
        );

        Task<ResultDTO<List<TransactionInfoResponse>>> GetAllTransactionsByProjectId(Guid? projectId, TransactionFilter filter = TransactionFilter.All);
        Task<ResultDTO<IEnumerable<object>>> GetAllTransactionsByMarketId(Guid? projectId);
    }
}
