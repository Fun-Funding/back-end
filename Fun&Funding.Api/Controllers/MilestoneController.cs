using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.MilestoneDTO;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/milestones")]
    [ApiController]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneService _milestoneService;

        public MilestoneController(IMilestoneService milestoneService)
        {
            _milestoneService = milestoneService;
        }
        [HttpGet("group-latest-milestone")]
        public async Task<IActionResult> GetAllMilestoneOrder([FromQuery] bool? status, MilestoneFilter filter = MilestoneFilter.All)
        {
            var result = await _milestoneService.GetListLastestMilestone(status,filter);
            if (!result._isSuccess)
                return Ok(StatusCode(result._statusCode, result));
            return Ok(result);
        }
        [HttpGet("verson-or-order")]
        public async Task<IActionResult> GetMilestoneByVersionOrOrder([FromQuery] int? Order, int? Version)
        {
            var result = await _milestoneService.GetMilestoneByVersionAndOrder(Order,Version);
            if (!result._isSuccess)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewMilestone(AddMilestoneRequest request)
        {
            var result = await _milestoneService.CreateMilestone(request);
            if (!result._isSuccess)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMilestoneById(Guid id, [FromQuery] int? filter = 0)
        {
            var result = _milestoneService.GetMilestoneById(id, filter);
            return Ok(result);
        }
    }
}
