using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.MarketplaceProjectDTO
{
    public class MarketplaceProjectOrderResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public required virtual ICollection<MarketplaceFileInfoResponse> MarketplaceFiles { get; set; }
    }
}
