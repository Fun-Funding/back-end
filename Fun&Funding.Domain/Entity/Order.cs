using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
