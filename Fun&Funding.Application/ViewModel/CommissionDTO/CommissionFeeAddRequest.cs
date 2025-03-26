using Fun_Funding.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Application.ViewModel.CommissionDTO
{
    public class CommissionFeeAddRequest
    {
        [Required(ErrorMessage = "Rate is required.")]
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }
        [Required(ErrorMessage = "Commission Type is required.")]
        public CommissionType CommissionType { get; set; }
        [Required(ErrorMessage = "Version is required.")]
        public string Version { get; set; } = string.Empty;
    }
}
