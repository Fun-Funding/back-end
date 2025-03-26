using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Application.ViewModel.TransactionDTO;
using Fun_Funding.Application.ViewModel.WithdrawDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.WalletDTO
{
    public class WalletInfoResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Balance { get; set; }
        public ICollection<TransactionInfoResponse>? Transactions { get; set; }
        public ICollection<WithdrawResponse>? WithdrawRequests { get; set; }

    }
}
