using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.OrderDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/digital-keys")]
    [ApiController]
    public class DigitalKeyController : ControllerBase
    {
        public IDigitalKeyService _digitalKeyService;
        public DigitalKeyController(IDigitalKeyService digitalKeyService)
        {
            _digitalKeyService = digitalKeyService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllDigitalKey([FromQuery] ListRequest request) {
            var response = await _digitalKeyService.GetAllDigitalKey(request);
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDigitalKeyById(Guid id)
        {
            var response = await _digitalKeyService.GetDigitalKeyById(id);
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> GenerateTestKey([FromQuery] Guid marketplaceProjectId)
        {
            var response = await _digitalKeyService.GenerateTestKey(marketplaceProjectId);
            return Ok(response);
        }
        [HttpPatch]
        public async Task<IActionResult> VerifyDigitalKey(string key, string projectName)
        {
            var response = await _digitalKeyService.VerifyDigitalKey(key, projectName);
            return Ok(response);
        }
    }
}
