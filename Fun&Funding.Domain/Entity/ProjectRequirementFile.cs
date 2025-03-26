using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class ProjectRequirementFile : BaseEntity
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public FileType File { get; set; }

        public Guid ProjectMilestoneRequirementId { get; set; } 
        public ProjectMilestoneRequirement ProjectRequirement { get; set; }

    }
}
