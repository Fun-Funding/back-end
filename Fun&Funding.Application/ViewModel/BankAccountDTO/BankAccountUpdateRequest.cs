using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.BankAccountDTO
{
    public class BankAccountUpdateRequest
    {
        public Guid Id { get; set; }
        public string? BankNumber { get; set; }
        public string? BankCode { get; set; }
    }
}
