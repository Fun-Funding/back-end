using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class EvidenceImage : BaseEntity
    {
        public string Url { get; set; } 
        public Guid PackageBackerId { get; set; }
        public PackageBacker? PackageBacker { get; set; }
    }
}
