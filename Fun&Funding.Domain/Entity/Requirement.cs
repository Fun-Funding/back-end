using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class Requirement : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public int Order { get; set; }
        public Guid MilestoneId { get; set; }
        public Milestone Milestone { get; set; }
        public FixedRequirementStatus Status { get; set; }
        public virtual ICollection<ProjectMilestoneRequirement>? ProjectRequirements { get; set; }

    }
}
