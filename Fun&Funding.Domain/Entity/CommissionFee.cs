using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class CommissionFee : BaseEntity
    {
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }
        public CommissionType CommissionType { get; set; }
        public string Version { get; set; }
        public DateTime UpdateDate { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }

    }
}
