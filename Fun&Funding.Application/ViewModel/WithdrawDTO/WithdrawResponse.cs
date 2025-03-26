using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.WithdrawDTO
{
    public class WithdrawResponse
    {
        public decimal Amount { get; set; }
        public bool IsFinished { get; set; }
        public TransactionTypes RequestType { get; set; }
        public WithdrawRequestStatus Status { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid? WalletId { get; set; }
    }
}
