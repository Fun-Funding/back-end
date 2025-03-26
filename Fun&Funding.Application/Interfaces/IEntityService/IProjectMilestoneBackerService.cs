using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ProjectMilestoneBackerDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface IProjectMilestoneBackerService
    {
        Task<ResultDTO<ProjectMilestoneBackerResponse>> CreateNewProjectMilestoneBackerReview(ProjectMilestoneBackerRequest request);
        Task<ResultDTO<List<ProjectMilestoneBackerResponse>>> GetAllMilestoneReview(Guid projectMilestoneId);
        Task<ResultDTO<bool>> CheckIfQualifiedForReview(Guid projectMilestoneId, Guid userId);
    }
}
