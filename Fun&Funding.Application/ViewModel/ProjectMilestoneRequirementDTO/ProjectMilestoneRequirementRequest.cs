using Fun_Funding.Application.ViewModel.ProjectRequirementFileDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.ProjectMilestoneRequirementDTO
{
    public class ProjectMilestoneRequirementRequest
    {
        public RequirementStatus RequirementStatus { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? Content { get; set; }
        public Guid MilestoneId { get; set; }
        public Guid RequirementId { get; set; }
        public Guid FundingProjectId { get; set; }
        public List<ProjectRequirementFileRequest>? RequirementFiles { get; set; }
    }
}
