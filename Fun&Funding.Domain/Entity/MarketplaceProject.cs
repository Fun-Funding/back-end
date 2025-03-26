using Fun_Funding.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Domain.Entity
{
    public class MarketplaceProject : BaseEntity
    {
        public string Introduction { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public string? Note { get; set; }
        public FundingProject FundingProject { get; set; }
        public Guid FundingProjectId { get; set; }
        public ProjectStatus Status { get; set; }
        public Wallet? Wallet { get; set; }
        public virtual ICollection<DigitalKey>? DigitalKeys { get; set; }
        public virtual ICollection<MarketplaceFile>? MarketplaceFiles { get; set; }
        public virtual ICollection<ProjectCoupon>? ProjectCoupons { get; set; }
    }
}
