using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.CouponDTO
{
    public class ListCouponResponse
    {
        public int numOfCoupon {  get; set; }
        public List<CouponResponse> List { get; set; }
    }
}
