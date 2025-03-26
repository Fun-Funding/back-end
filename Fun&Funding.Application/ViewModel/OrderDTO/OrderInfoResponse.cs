using Fun_Funding.Application.ViewModel.OrderDetailDTO;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.OrderDTO
{
    public class OrderInfoResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalPrice { get; set; }
        public UserInfoResponse? User {  get; set; }
        public ICollection<OrderDetailInfoResponse>? OrderDetails { get; set; }
    }
}
