using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Application.ViewModel.CommissionDTO
{
    public class CommissionFeeUpdateRequest
    {
        [Required(ErrorMessage = "Rate is required.")]
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }
    }
}
