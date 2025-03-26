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
    public class ProjectMilestone : BaseEntity
    {
        public DateTime EndDate { get; set; }
        public ProjectMilestoneStatus Status { get; set; }
        public Guid MilestoneId { get; set; }
        public Milestone Milestone { get; set; }
        public Guid FundingProjectId { get; set; }
        public string? IssueLog { get; set; }
        public string? Title { get; set; }
        public string? Introduction { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalAmount { get; set; }
        public FundingProject FundingProject { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<ProjectMilestoneBacker> ProjectMilestoneBackers { get; set; }
        public virtual ICollection<ProjectMilestoneRequirement> ProjectMilestoneRequirements { get; set; }

        public string? Note { get; set; }
    }
}
