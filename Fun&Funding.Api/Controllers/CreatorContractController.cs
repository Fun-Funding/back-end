using Fun_Funding.Application;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.CreatorContractDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/creator-contracts")]
    [ApiController]
    public class CreatorContractController : ControllerBase
    {
        private readonly ICreatorContractService _contractService;
        public CreatorContractController(ICreatorContractService contractService) {
           _contractService = contractService;
        }
        [HttpPost]
        [Authorize]
        public IActionResult CreateContract()
        {
            var result = _contractService.CreateContract();
            return Ok(result);
        }
    }
}
