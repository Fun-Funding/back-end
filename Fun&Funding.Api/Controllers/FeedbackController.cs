using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FeedbackDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFeedback([FromQuery] ListRequest request)
        {
            var result = await _feedbackService.GetAllFeedback(request);
            if (!result._isSuccess)
            {
                NotFound();
            }
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(Guid id)
        {
            var result = await _feedbackService.GetFeedbackById(id);
            if (!result._isSuccess) return NotFound();
            return Ok(result);
        }
        [HttpPatch("{id}/approved")]
        public async Task<IActionResult> ApprovedFeedBack(Guid id)
        {
            var result = await _feedbackService.ApprovedById(id);
            if (!result._isSuccess) return NotFound(result);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFeedback(FeedbackRequest resquest)
        {
            var result = await _feedbackService.CreateFeedBack(resquest);
            if (!result._isSuccess)
            {
                NotFound();
            }
            return Ok(result);
        }
        [HttpGet("top4")]
        public async Task<IActionResult> GetTop4Feedback()
        {
            var result = await _feedbackService.GetTop4Feedback();
            if (!result._isSuccess) return NotFound(result);
            return Ok(result);
        }
    }
}
