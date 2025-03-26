using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.LikeDTO
{
    public class MarketplaceLikeNumbers
    {
        public int LikeCount { get; set; }
        public List<MarketplaceProjectInfoResponse>? marketplaceProjectResponses { get; set; }
    }
}
