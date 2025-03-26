using Fun_Funding.Application.ViewModel.BankAccountDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.WalletDTO
{
    public class WalletFundingResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Balance { get; set; }

        public BankAccountInfoResponse? BankAccount {  get; set; }
    }
}
