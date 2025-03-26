using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class BankAccount : BaseEntity
    {
        public string? BankNumber { get; set; }
        public string? BankCode { get; set; }
        
        public Wallet? Wallet { get; set; }
    }
}
