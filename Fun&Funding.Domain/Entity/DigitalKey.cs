using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class DigitalKey : BaseEntity
    {
        public string? KeyString { get; set; }
        public KeyStatus Status { get; set; }
        public DateTime ExpiredDate { get; set; }
        public MarketplaceProject MarketplaceProject { get; set; }

    }
}
