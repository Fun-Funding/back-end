using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.ProjectMilestoneDTO
{
    public class ProjectMilestoneStatusUpdateRequest
    {
        public Guid ProjectMilestoneId { get; set; }
        public ProjectMilestoneStatus Status { get; set; }
        public DateTime? NewEndDate { get; set; }
    }
}
