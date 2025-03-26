using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class MarketplaceFile : BaseEntity
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public FileType FileType { get; set; }
        public Guid? MarketplaceProjectId { get; set; }
    }
}
