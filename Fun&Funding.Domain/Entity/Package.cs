using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class Package : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RequiredAmount { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LimitQuantity { get; set; }
        public PackageType PackageTypes { get; set; }
        public PackageStatus PackageStatus { get; set; }
        [Required]
        [ForeignKey(nameof(Project))]
        public Guid ProjectId { get; set; }
        public FundingProject? Project { get; set; } 
        public virtual ICollection<RewardItem> RewardItems { get; set; }
        public virtual ICollection<PackageBacker> PackageUsers { get; set; }
        
    }
}
