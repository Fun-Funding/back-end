using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Fun_Funding.Application.ViewModel.MarketplaceFileDTO
{
    public class MarketplaceGameFileRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
        public required IFormFile URL { get; set; }
        [Required(ErrorMessage = "Version is required.")]
        public string Version { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;
    }
}
