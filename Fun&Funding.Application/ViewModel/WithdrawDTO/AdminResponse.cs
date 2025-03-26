using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.WithdrawDTO
{
    public class AdminResponse
    {
        public Guid AdminId { get; set; }
        public string? BankNumber { get; set; }
        public string? BankCode { get; set; }
        public WithdrawRequest WithdrawRequest { get; set; }

    }
}
