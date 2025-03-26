using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Application.ViewModel.ProjectMilestoneRequirementDTO;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.IService
{
    public interface IProjectMilestoneRequirementService
    {
        Task<ResultDTO<ProjectMilestoneResponse>> CreateMilestoneRequirements(List<ProjectMilestoneRequirementRequest> request, string? issueLog);
        Task<ResultDTO<string>> UpdateMilestoneRequirements(List<ProjectMilestoneRequirementUpdateRequest> request, string? issueLog);
    }
}
