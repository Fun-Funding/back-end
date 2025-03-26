using Fun_Funding.Application.ViewModel.CategoryDTO;
using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Application.ViewModel.WalletDTO;
using Fun_Funding.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Application.ViewModel.MarketplaceProjectDTO
{
    public class MarketplaceProjectInfoResponse
    {
        public Guid Id { get; set; }
        public string? Introduction { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public required UserInfoResponse User { get; set; }
        public required virtual ICollection<MarketplaceFileInfoResponse> MarketplaceFiles { get; set; }
        public required virtual ICollection<CategoryResponse> Categories { get; set; }
        public ProjectStatus Status { get; set; }
        public required WalletFundingResponse Wallet { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Note { get; set; }
    }
}
