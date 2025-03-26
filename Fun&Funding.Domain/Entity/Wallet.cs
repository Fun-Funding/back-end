using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class Wallet : BaseEntity
    {
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        [ForeignKey("BackerId")]
        public User? Backer { get; set; }
        [ForeignKey("FundingProjectId")]
        public FundingProject? FundingProject { get; set; }
        [ForeignKey("MarketplaceProjectId")]
        public MarketplaceProject? MarketplaceProject { get; set; }
        [ForeignKey(nameof(BankAccount))]
        public Guid BankAccountId { get; set; }

        public BankAccount? BankAccount { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<WithdrawRequest>? WithdrawRequests { get; set; }
    }
}
