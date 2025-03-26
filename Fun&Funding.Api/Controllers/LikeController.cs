using Fun_Funding.Api.Exception;
using Fun_Funding.Application;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.LikeDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fun_Funding.Api.Controllers
{
    [Route("api/likes")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly IUnitOfWork _unitOfWork;


        public LikeController(ILikeService likeService, IUnitOfWork unitOfWork)
        {
            _likeService = likeService;
            _unitOfWork = unitOfWork;

        }

        [HttpGet("number-of-marketplace-like")]
        public async Task<IActionResult> GetMarketplaceLike()
        {
            var result = await _likeService.NumberOfMarketplaceLike();
            return Ok(result);
        } 
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _likeService.GetAll();
            return Ok(result);
        }
        [HttpPost("funding")]
        public async Task<IActionResult> likeFundingProject([FromBody] LikeRequest likeRequest)
        {
            var result = await _likeService.LikeFundingProject(likeRequest);
            if (!result._isSuccess)
            {
                return BadRequest(result._message);
            }
            return Ok(result);
        }
        [HttpPost("marketplace")]
        public async Task<IActionResult> likeMarketplaceProject([FromBody] LikeRequest likeRequest)
        {
            var result = await _likeService.LikeMarketplaceProject(likeRequest);
            if (!result._isSuccess)
            {
                return BadRequest(result._message);
            }
            return Ok(result);
        }
       
        [HttpGet("get-project-like/{id}")]
        public async Task<IActionResult> CheckProjectLike(Guid id)
        {
            try
            {
                var result = await _likeService.CheckUserLike(id);
                return Ok(result);
            }
            catch (ExceptionError ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
