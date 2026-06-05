using eCommerce.Application.DTOs.Products.Variants;
using eCommerce.Application.Services.Interfaces.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariantController(IVariantService variantService) : ControllerBase
    {
        [HttpPost("generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateAsync(GenerateVariantsDTO dto)
        {
            var result = await variantService.GenerateAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("byId/{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByProductIdAsync(Guid productId)
            => Ok(await variantService.GetByProductIdAsync(productId));

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(UpdateVariantDTO dto)
        {
            var result = await variantService.UpdateVariantAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
