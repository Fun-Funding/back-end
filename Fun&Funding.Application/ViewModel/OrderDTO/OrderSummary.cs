using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.OrderDTO
{
    public class OrderSummary
    {
        public DateTime CreatedDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
