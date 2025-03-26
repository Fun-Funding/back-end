using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.CouponDTO
{
    public class CouponResponse
    {
        public string CouponKey { get; set; }
        public string CouponName { get; set; }
        public decimal DiscountRate { get; set; }
        public ProjectCouponStatus Status { get; set; }
        public Guid? MarketplaceProjectId { get; set; }
    }
}
