using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.ProjectMilestoneRequirementDTO;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/project-milestone-requirements")]
    [ApiController]
    public class ProjectMilestoneRequirementController : ControllerBase
    {
        private IProjectMilestoneRequirementService _projectMilestoneRequirementService;
        public ProjectMilestoneRequirementController(IProjectMilestoneRequirementService projectMilestoneRequirementService) 
        {
            _projectMilestoneRequirementService = projectMilestoneRequirementService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMilestoneRequirements([FromForm] List<ProjectMilestoneRequirementRequest> request, string? issueLog)
        {
            var result = await _projectMilestoneRequirementService.CreateMilestoneRequirements(request, issueLog);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMilestoneRequirements([FromForm] List<ProjectMilestoneRequirementUpdateRequest> request, [FromForm] string? issueLog)
        {
            var result = await _projectMilestoneRequirementService.UpdateMilestoneRequirements(request,issueLog);
            return Ok(result);
        }
    }
}
