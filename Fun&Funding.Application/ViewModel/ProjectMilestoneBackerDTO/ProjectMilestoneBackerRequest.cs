using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Application.ViewModel.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.ProjectMilestoneBackerDTO
{
    public class ProjectMilestoneBackerRequest
    {
        public decimal Star { get; set; }
        public string Comment { get; set; }
        public Guid BackerId { get; set; }
        public Guid ProjectMilestoneId { get; set; }
    }
}
