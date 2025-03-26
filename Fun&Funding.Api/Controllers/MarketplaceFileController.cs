using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;
using Fun_Funding.Domain.Constrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/marketplace-files")]
    [ApiController]
    public class MarketplaceFileController : ControllerBase
    {
        private readonly IMarketplaceFileService _marketplaceFileService;

        public MarketplaceFileController(IMarketplaceFileService marketplaceFileService)
        {
            _marketplaceFileService = marketplaceFileService;
        }

        [HttpGet("{marketplaceProjectId}/game-files")]
        public async Task<IActionResult> GetGameFiles([FromRoute] Guid marketplaceProjectId,
            [FromQuery] ListRequest request)
        {
            var response = await _marketplaceFileService.GetGameFiles(marketplaceProjectId, request);
            return Ok(response);
        }

        [Authorize(Roles = Role.GameOwner)]
        [HttpPost("{marketplaceProjectId}/game-files")]
        public async Task<IActionResult> UploadGameUpdateFile([FromRoute] Guid marketplaceProjectId,
            [FromForm] MarketplaceGameFileRequest request)
        {
            var response = await _marketplaceFileService.UploadGameUpdateFile(marketplaceProjectId, request);
            return new ObjectResult(response)
            {
                StatusCode = response._statusCode
            };
        }
    }
}
