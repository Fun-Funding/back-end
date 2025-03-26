using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.CartDTO
{
    public class ItemInfoResponse
    {
        public MarketplaceProjectInfoResponse? MarketplaceProject;
        public DateTime? CreatedDate { get; set; }
    }
}
