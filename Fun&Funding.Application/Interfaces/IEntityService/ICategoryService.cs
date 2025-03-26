using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CategoryDTO;

namespace Fun_Funding.Application.IService
{
    public interface ICategoryService
    {
        Task<ResultDTO<PaginatedResponse<CategoryResponse>>> GetCategories(ListRequest request);
        Task<ResultDTO<CategoryResponse>> GetCategoryById(Guid id);
        Task<ResultDTO> DeleteCategory(Guid id);
        Task<ResultDTO<CategoryResponse>> CreateCategory(CategoryRequest request);
        Task<ResultDTO<CategoryResponse>> UpdateCategory(Guid id, CategoryRequest request);
        Task<ResultDTO<IEnumerable<CategoryResponse>>> GetAllCategories();
    }
}
