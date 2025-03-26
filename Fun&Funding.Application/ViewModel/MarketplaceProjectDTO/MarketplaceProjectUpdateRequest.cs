using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Application.ViewModel.MarketplaceFileDTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Application.ViewModel.MarketplaceProjectDTO
{
    public class MarketplaceProjectUpdateRequest
    {
        public string Introduction { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public List<MarketplaceFileRequest>? MarketplaceFiles { get; set; }
        public List<MarketplaceFileInfoResponse>? ExistingFiles { get; set; }
        public BankAccountUpdateRequest BankAccount { get; set; }
    }
}
