using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class PackageBacker : BaseEntity
    {
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DonateAmount { get; set; }

        public List<EvidenceImage>? EvidenceImages { get; set; }
        [Required]
        public Guid PackageId { get; set; }
        public Package Package { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
