using Fun_Funding.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Domain.Entity
{
    public class FundingProject : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Target { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }
        public ProjectStatus Status { get; set; }

        public string? Note { get; set; } = string.Empty;

        public Wallet? Wallet { get; set; }
        public MarketplaceProject? MarketplaceProject { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }

        public virtual ICollection<FundingFile> SourceFiles { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Package> Packages { get; set; }
        public virtual ICollection<ProjectMilestone>? ProjectMilestones { get; set; }
    }
}
