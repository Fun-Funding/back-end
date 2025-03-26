using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class OrderDetail : BaseEntity
    {
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }
        [Required]
        public Guid DigitalKeyID { get; set; }
        [Required]
        public DigitalKey DigitalKey { get; set; }

        [Required]
        public Guid OrderId { get; set; }
        [Required]
        public Order Order { get; set; }
        public Guid? ProjectCouponId { get; set; }
        public ProjectCoupon? ProjectCoupon { get; set; }
    }
}
