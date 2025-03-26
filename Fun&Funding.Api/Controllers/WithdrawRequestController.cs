using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.WithdrawDTO;
using Fun_Funding.Domain.Constrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/withdraw-requests")]
    [ApiController]
    public class WithdrawRequestController : ControllerBase
    {
        private readonly IWithdrawService _withdrawService;

        public WithdrawRequestController(IWithdrawService withdrawService)
        {
            _withdrawService = withdrawService;
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRequest([FromQuery]ListRequest request)
        {
            var result = await _withdrawService.GetAllRequest(request);
            if(result == null) return NotFound();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(Guid id)
        {
            var result = await _withdrawService.GetWithdrawRequestById(id);
            if(result == null) return NotFound();
            return Ok(result);
        }
        [HttpPost("marketplace/{marketplaceId}")]
        [Authorize(Roles = Role.GameOwner)]
        public async Task<IActionResult> CreateMarketplaceRequest(Guid marketplaceId)
        {
            var result = await _withdrawService.CreateMarketplaceRequest(marketplaceId);
            return Ok(result);
        }
        [HttpPatch("{id}/process")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ProcessingRequest(Guid id)
        {
            var result = await _withdrawService.AdminProcessingRequest(id);
            if(result == null) return NotFound();
            return Ok(result);
        }
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> ApprovedRequest(Guid id)
        {
            var result = await _withdrawService.AdminApproveRequest(id);
            return Ok(result);
        }
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(Guid id)
        {
            var result = await _withdrawService.AdminCancelRequest(id);
            return Ok(result);
        }
        [HttpPost("wallet-request")]
        public async Task<IActionResult> WalletWithdrawRequest(decimal amount)
        {
            var result = await _withdrawService.WalletWithdrawRequest(amount);
            return Ok(result);
        }
    }
}
