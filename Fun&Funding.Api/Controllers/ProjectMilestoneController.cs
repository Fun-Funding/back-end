using Fun_Funding.Api.Exception;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/project-milestones")]
    [ApiController]
    public class ProjectMilestoneController : ControllerBase
    {
        private IProjectMilestoneService _projectMilestoneService;
        public ProjectMilestoneController(IProjectMilestoneService projectMilestoneService) {
            _projectMilestoneService = projectMilestoneService;
        }
        //api create milestone request
        [HttpPost]
        public async Task<IActionResult> CreateMilestoneRequest([FromBody]ProjectMilestoneRequest request)
        {
            try
            {
                var res = await _projectMilestoneService.CreateProjectMilestoneRequest(request);
                return Ok(res);
            }
            catch(ExceptionError ex)
            {
                return Ok(ex.InnerException);
            }
           
        }

        // api get list projectmilestone
        [HttpGet]
        public async Task<IActionResult> GetMilestones([FromQuery] ListRequest request , ProjectMilestoneStatus? status, Guid? projectId, Guid? milestoneId)
        {
                var res = await _projectMilestoneService.GetProjectMilestones(request, status, projectId, milestoneId);
                return Ok(res);
        }

        // api update status projectmilestone
        [HttpPut]
        public async Task<IActionResult> UpdateProjectMilestone([FromBody] ProjectMilestoneStatusUpdateRequest req)
        {
            var res = await _projectMilestoneService.UpdateProjectMilestoneStatus(req);
            return Ok(res);
        }

        // api get detail projectmilestone
        [HttpGet("{id}")]
        public IActionResult GetProjectMilestoneById(Guid id)
        {
            var res = _projectMilestoneService.GetProjectMilestoneRequest(id);
            return Ok(res);
        }

        [HttpGet("milestones-disbursement")]
        public async Task<IActionResult> GetMilestonesByProjectdAndMilestone(Guid? projectId, Guid? milestoneId)
        {
            var res = await _projectMilestoneService.GetProjectMilestonesByProjectAndMilestone(projectId, milestoneId);
            return Ok(res);
        }

        [HttpPost("withdraw-process")]
        public async Task<IActionResult> WithdrawMilestone(Guid projectMilestoneId)
        {
            var result = await _projectMilestoneService.WithdrawMilestoneProcessing(projectMilestoneId);
            return Ok(result);
        }

    }
}
