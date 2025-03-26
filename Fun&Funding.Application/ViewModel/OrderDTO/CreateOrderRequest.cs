using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.OrderDTO
{
    public class CreateOrderRequest
    {
        public DateTime CreatedDate { get; set; }
        [Required]
        public List<CartItem>? CartItems { get; set; }
    }
}
