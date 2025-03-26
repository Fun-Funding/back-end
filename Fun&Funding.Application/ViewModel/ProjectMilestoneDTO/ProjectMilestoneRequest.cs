using Fun_Funding.Application.ViewModel.ProjectMilestoneRequirementDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.ProjectMilestoneDTO
{
    public class ProjectMilestoneRequest
    {
        public ProjectMilestoneStatus Status { get; set; }
        public Guid MilestoneId { get; set; }
        public Guid FundingProjectId { get; set; }
        public string? Title { get; set; }
        public string? Introduction { get; set; }
        public decimal? TotalAmount { get; set; }

        //public List<ProjectMilestoneRequirementRequest> ProjectMilestoneRequirements { get; set; }
    }
}
