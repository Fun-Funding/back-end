using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ReportDTO;
using Fun_Funding.Domain.Constrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListRequest request)
        {
            var result = await _reportService.GetAllReport(request);
            if (!result._isSuccess)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost]       
        public async Task<IActionResult> CreateReport([FromForm]ReportRequest request)
        {
            var result = await _reportService.CreateReportRequest(request);
            if (!result._isSuccess)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("send-email")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> SendEmail([FromBody] EmailReportRequest request)
        {
            var result = await _reportService.SendReportedEmail(request);
            if (!result._isSuccess)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPatch]
        [Authorize(Roles =Role.Admin)]
        public async Task<IActionResult> UpdateReport(Guid id)
        {
            var result = await _reportService.UpdateHandleReport(id);
            if (!result._isSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
