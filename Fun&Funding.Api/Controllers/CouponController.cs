using Fun_Funding.Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/coupons")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly IProjectCouponService _couponService;

        public CouponController(IProjectCouponService couponService)
        {
            _couponService = couponService;
        }
        [HttpGet("all/{id}")]
        public async Task<IActionResult> GetCouponsByProjectId(Guid id)
        {
            var result = await _couponService.GetListCouponByProjectId(id);
            if (!result._isSuccess) return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("get-by-code")]
        public async Task<IActionResult> GetCouponsByCode(string couponCode, Guid marketplaceProjectId)
        {
            var result = await _couponService.GetCouponByCode(couponCode, marketplaceProjectId);
            if (!result._isSuccess) return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("check-avaliable-coupons")]
        public async Task<IActionResult> CheckCouponValid(string couponCode, Guid marketplaceProjectId)
        {
            var result = await _couponService.CheckCouponValid(couponCode, marketplaceProjectId);
            if (!result._isSuccess) return BadRequest(result);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> ImportFile(IFormFile formFile, Guid projectId)
        {
            var result = await _couponService.ImportFile(formFile, projectId);
            if(!result._isSuccess) return BadRequest(result);
            return Ok(result);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeStatusCoupons(Guid id)
        {
            var result = await _couponService.ChangeStatusCoupons(id);
            if(!result._isSuccess) return BadRequest(result);
            return Ok(result);
        } 
        [HttpPatch]
        public async Task<IActionResult> ChangeStatus(Guid couponId)
        {
            var result = await _couponService.ChangeStatus(couponId);
            if(!result._isSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
