using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.CartDTO
{
    public class CartInfoResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<ItemInfoResponse>? Items { get; set; }
    }
}
