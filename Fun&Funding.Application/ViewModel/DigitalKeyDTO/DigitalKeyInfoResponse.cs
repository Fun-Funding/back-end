using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.DigitalKeyDTO
{
    public class DigitalKeyInfoResponse
    {
        public Guid Id { get; set; }
        public string? KeyString { get; set; }
        public KeyStatus Status { get; set; }
        public DateTime ExpiredDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public MarketplaceProjectOrderResponse? MarketingProject { get; set; }
    }
}
