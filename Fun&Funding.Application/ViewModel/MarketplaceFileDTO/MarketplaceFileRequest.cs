using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace Fun_Funding.Application.ViewModel.MarketplaceFileDTO
{
    public class MarketplaceFileRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Version { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public required IFormFile URL { get; set; }
        public FileType FileType { get; set; }
    }
}
