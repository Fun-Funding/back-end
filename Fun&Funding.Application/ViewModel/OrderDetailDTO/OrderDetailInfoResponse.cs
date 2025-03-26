using Fun_Funding.Application.ViewModel.CouponDTO;
using Fun_Funding.Application.ViewModel.DigitalKeyDTO;
using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.OrderDetailDTO
{
    public class OrderDetailInfoResponse
    {
        public Guid? OrderId { get; set; }
        public decimal UnitPrice { get; set; }

        public DigitalKeyInfoResponse? DigitalKey { get; set; }
        public CouponResponse? ProjectCoupon { get; set; }
    }
}
