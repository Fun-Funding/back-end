using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.BankAccountDTO
{
    public class BankAccountInfoResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? BankNumber { get; set; }
        public string? BankCode { get; set; }
    }
}
