using System.ComponentModel.DataAnnotations;

namespace Fun_Funding.Application.ViewModel.CategoryDTO
{
    public class CategoryRequest
    {
        [Required(ErrorMessage = "Category Name is required.")]
        public string Name { get; set; } = string.Empty;
    }
}
