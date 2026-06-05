using eCommerce.Application.DTOs.Products;
using eCommerce.Application.Services.Interfaces.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProductDTO productDTO)
        {
            var result = await productService.CreateAsync(productDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateProductDTO productDTO)
        {
            var result = await productService.UpdateAsync(productDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await productService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await productService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAsync([FromQuery] int days = 7)
        {
            var result = await productService.GetRecentAsync(days);
            return Ok(result);
        }

        [HttpGet("byId/{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await productService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("categoryId/{categoryId}")]
        public async Task<IActionResult> GetByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await productService.GetByCategoryAsync(categoryId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync([FromQuery] string term)
        {
            var result = await productService.SearchAsync(term);
            return Ok(result);
        }

        [HttpPut("bestSeller/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetBestSeller(Guid id, [FromBody] bool value)
        {
            var result = await productService.SetBestSellerAsync(id, value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(UploadImageDTO request)
        {
            var result = await productService.UploadImagesAsync(request.ProductId, request.File);

            return result.Success
                ? Ok(result.Data)
                : BadRequest(result.Message);
        }

        [HttpPatch("image/position")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateImagePosition([FromBody] UpdateImagePositionDTO dto)
        {
            var result = await productService.UpdateImagePositionAsync(dto.ProductId, dto.ImageUrl, dto.ObjectPosition);
            return result.Success ? Ok() : BadRequest(result.Message);
        }

        [HttpDelete("image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage([FromQuery] Guid productId, [FromQuery] string imageUrl)
        {
            var result = await productService.RemoveImageAsync(productId, imageUrl);
            return result.Success ? NoContent() : BadRequest(result);
        }
    }
}
