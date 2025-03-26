using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.CategoryDTO
{
    public class CategoryProjectRequest
    {
        [Required]
        public Guid Id { get; set; }
    }
}
