using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CategoryDTO;
using Fun_Funding.Domain.Constrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controller
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] ListRequest request)
        {
            var response = await _categoryService.GetCategories(request);
            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories()
        {
            var response = await _categoryService.GetAllCategories();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] Guid id)
        {
            var response = await _categoryService.GetCategoryById(id);
            return Ok(response);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            var response = await _categoryService.DeleteCategory(id);
            return NoContent();
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
        {
            var response = await _categoryService.CreateCategory(request);
            return new ObjectResult(response)
            {
                StatusCode = response._statusCode
            };
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] CategoryRequest request)
        {
            var response = await _categoryService.UpdateCategory(id, request);
            return Ok(response);
        }
    }
}
