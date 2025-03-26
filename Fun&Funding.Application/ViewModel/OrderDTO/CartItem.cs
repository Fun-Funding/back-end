using Fun_Funding.Application.ViewModel.CouponDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.OrderDTO
{
    public class CartItem
    {
        public Guid MarketplaceProjectId { get; set; }
        public decimal Price { get; set; }
        public AppliedCoupon? AppliedCoupon { get; set; }
    }
}
