using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CommissionDTO;
using Fun_Funding.Domain.Constrain;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    
    [Route("api/commision-fees")]
    [ApiController]
    public class CommissionFeeController : ControllerBase
    {
        private readonly ICommissionFeeService _commissionFeeService;

        public CommissionFeeController(ICommissionFeeService commissionFeeService)
        {
            _commissionFeeService = commissionFeeService;
        }

        [HttpGet("list-applied-commission-fee")]
        public IActionResult GetAppliedCommissionFee()
        {
            var response = _commissionFeeService.GetListAppliedCommissionFee();
            return Ok(response);
        }

        [HttpGet("latest-commission-fee")]
        public IActionResult GetLatestCommissionFee(CommissionType type)
        {
            var response = _commissionFeeService.GetAppliedCommissionFee(type);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetCommissionFees([FromQuery] ListRequest request, [FromQuery] CommissionType? type)
        {
            var response = await _commissionFeeService.GetCommissionFees(request, type);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> CreateCommissionFee([FromBody] CommissionFeeAddRequest request)
        {
            var response = await _commissionFeeService.CreateCommissionFee(request);
            return new ObjectResult(response)
            {
                StatusCode = response._statusCode
            };
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateCommissionFee([FromRoute] Guid id, [FromBody] CommissionFeeUpdateRequest request)
        {
            var response = await _commissionFeeService.UpdateCommsisionFee(id, request);
            return Ok(response);
        }
    }
}
