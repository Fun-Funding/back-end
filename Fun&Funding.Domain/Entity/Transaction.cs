using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Domain.Entity
{
    public class Transaction : BaseEntity
    {       
        public string Description { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        public TransactionTypes TransactionType { get; set; }

        public Guid? PackageId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? OrderDetailId { get; set; }
        public Guid? ProjectMilestoneId { get; set; }
        public ProjectMilestone? ProjectMilestone { get; set; }
        public Guid? WalletId { get; set; }
        public Wallet? Wallet { get; set; }
        public Guid? SystemWalletId { get; set; }
        public SystemWallet? SystemWallet { get; set; }
        public Guid? CommissionFeeId { get; set; }
        public CommissionFee? CommissionFee { get; set; }
    }
}
