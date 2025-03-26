using Fun_Funding.Application.Interfaces.IExternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/storage")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IAzureService _azureService;

        public StorageController(IAzureService azureService)
        {
            _azureService = azureService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadSingleFile([FromForm] IFormFile file)
        {
            var result = await _azureService.UploadUrlSingleFiles(file);
            return Ok(result);
        }
    }
}
