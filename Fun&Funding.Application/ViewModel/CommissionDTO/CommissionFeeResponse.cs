using Fun_Funding.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fun_Funding.Application.ViewModel.CommissionDTO
{
    public class CommissionFeeResponse
    {
        public Guid Id { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }
        public CommissionType CommissionType { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime UpdateDate { get; set; }
    }
}
