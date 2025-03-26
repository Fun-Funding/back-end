using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FundingProjectDTO;
using Fun_Funding.Domain.Constrain;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/funding-projects")]
    [ApiController]
    public class FundingProjectController : ControllerBase
    {
        private IAzureService _storageService;
        private IFundingProjectService _fundingProjectService;
        public FundingProjectController(IAzureService storageService
            , IFundingProjectService fundingProjectService)
        {
            _storageService = storageService;
            _fundingProjectService = fundingProjectService;
        }

        [HttpPost]
        [Authorize(Roles = Role.GameOwner)]
        public async Task<IActionResult> CreateProject([FromForm] FundingProjectAddRequest req)
        {
            var response = await _fundingProjectService.CreateFundingProject(req);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var response = await _fundingProjectService.GetProjectById(id);
            return Ok(response);
        }

        [HttpGet("owner/{id}")]
        [Authorize(Roles = Role.GameOwner)]
        public async Task<IActionResult> GetProjectAndOwner(Guid id)
        {
            var response = await _fundingProjectService.GetProjectByIdAndOwner(id);
            return Ok(response);
        }

        [HttpPut]
        [Authorize(Roles = Role.GameOwner)]
        public async Task<IActionResult> UpdateProject([FromForm] FundingProjectUpdateRequest req)
        {
            var response = await _fundingProjectService.UpdateFundingProject(req);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetFundingProjects(
            [FromQuery] ListRequest request,
            [FromQuery] List<Guid>? categoryIds,
            [FromQuery] List<ProjectStatus>? statusList,
            [FromQuery] decimal? fromTarget,
            [FromQuery] decimal? toTarget)
        {
            var response = await _fundingProjectService.GetFundingProjects(request, categoryIds, statusList, fromTarget, toTarget);
            return Ok(response);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateProjectStatus([FromRoute] Guid id, [FromQuery] ProjectStatus status, [FromBody] string? note)
        {
            var response = await _fundingProjectService.UpdateFundingProjectStatus(id, status, note);
            return Ok(response);
        }

        [HttpGet("project-owner")]
        [Authorize]
        public async Task<IActionResult> CheckProjectOwner([FromQuery] Guid projectId)
        {
            var response = await _fundingProjectService.CheckProjectOwner(projectId);
            return Ok(response);
        }

        [HttpGet("top3")]
        public async Task<IActionResult> GetTop3MostFundedOngoingFundingProject()
        {
            var response = await _fundingProjectService.GetTop3MostFundedOngoingFundingProject();
            return Ok(response);
        }

        [HttpGet("game-owner-projects")]
        public async Task<IActionResult> GetGameOwnerFundingProjects([FromQuery] ListRequest request, string? categoryName, ProjectStatus? status, decimal? fromTarget, decimal? toTarget)
        {
            var response = await _fundingProjectService.GetGameOwnerFundingProjects(request, categoryName, status, fromTarget, toTarget);
            return Ok(response);
        }

        [HttpGet("backer-donation-projects")]
        public async Task<IActionResult> GetBackerDonatedProjects([FromQuery] ListRequest request, string? categoryName, ProjectStatus? status, decimal? fromTarget, decimal? toTarget)
        {
            var response = await _fundingProjectService.GetBackerDonatedProjects(request, categoryName, status, fromTarget, toTarget);
            return Ok(response);
        }

        [Authorize(Roles = Role.GameOwner)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFundingProject([FromRoute] Guid id)
        {
            await _fundingProjectService.DeleteFundingProject(id);
            return NoContent();
        }
    }
}