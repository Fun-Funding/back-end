using Fun_Funding.Api.Exception;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.CommentDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [HttpGet("all")]
        public async Task<IActionResult> AllCommentProjects()
        {
            var result = await _commentService.GetAllComment();
            return Ok(result);
        }
        [HttpPost("funding")]
        public async Task<IActionResult> CommentFundingProject([FromBody] CommentRequest request)
        {
            var result = await _commentService.CommentFundingProject(request);
            if (!result._isSuccess)
            {
                return BadRequest(result._message);
            }
            return Ok(result);
        }
        [HttpPost("marketplace")]
        public async Task<IActionResult> CommentMarketplaceProject([FromBody] CommentRequest request)
        {
            var result = await _commentService.CommentMarketplaceProject(request);
            if (!result._isSuccess)
            {
                return BadRequest(result._message);
            }
            return Ok(result);
        }
        [HttpGet("funding/{id}")]
        public async Task<IActionResult> GetFundingProjecComment(Guid id)

        {
            try
            {
                var result = await _commentService.GetCommentsByFundingProject(id);
                return Ok(result);
            }
            catch (ExceptionError ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("marketplace/{id}")]
        public async Task<IActionResult> GetMarketplaceProjecComment(Guid id)
        {
            try
            {
                var result = await _commentService.GetCommentsByMarketplaceProject(id);
                return Ok(result);
            }
            catch (ExceptionError ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjecComment(Guid id)
        {
            try
            {
                var result = await _commentService.DeleteComment(id);
                return Ok(result);
            }
            catch (ExceptionError ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
