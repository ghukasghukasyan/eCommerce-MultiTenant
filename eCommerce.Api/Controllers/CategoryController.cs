using eCommerce.Application.DTOs.Categories;
using eCommerce.Application.Services.Interfaces.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryDTO categoryDTO)
        {
            var result = await categoryService.CreateAsync(categoryDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateCategoryDTO categoryDTO)
        {
            var result = await categoryService.UpdateAsync(categoryDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await categoryService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await categoryService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("byId/{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await categoryService.GetByIdAsync(id);
            return Ok(result);
        }
    }
}

