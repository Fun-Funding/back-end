using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.RewardItemDTO
{
    public class ItemUpdateRequest
    {
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }
        //public string ImageUrl { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
