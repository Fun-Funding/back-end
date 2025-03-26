using Fun_Funding.Application.ViewModel.RewardItemDTO;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Fun_Funding.Application.ViewModel.PackageDTO
{
    public class PackageUpdateRequest
    {
        [Required]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RequiredAmount { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LimitQuantity { get; set; }

        public IFormFile? UpdatedImage { get; set; }
        public PackageType PackageTypes { get; set; }
        public virtual ICollection<ItemUpdateRequest> RewardItems { get; set; }
    }
}
