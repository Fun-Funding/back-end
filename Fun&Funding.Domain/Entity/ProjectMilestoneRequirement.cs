using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class ProjectMilestoneRequirement : BaseEntity
    {
        public RequirementStatus RequirementStatus { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Content { get; set; }

        public Guid RequirementId { get; set; }
        public Requirement Requirement { get; set; }
        public Guid ProjectMilestoneId {  get; set; } 
        public ProjectMilestone ProjectMilestone { get; set; }

        public virtual ICollection<ProjectRequirementFile>? RequirementFiles { get; set; }
    }
}
