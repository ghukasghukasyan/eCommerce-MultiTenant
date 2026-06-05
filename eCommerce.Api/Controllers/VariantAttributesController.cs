using eCommerce.Application.DTOs.Products.VariantAttributes;
using eCommerce.Application.Services.Interfaces.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class VariantAttributesController(IVariantAttributeService service) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync(CreateVariantAttributeDTO dto)
         => Ok(await service.CreateAsync(dto));

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync()
            => Ok(await service.GetAllAsync());
    }
}
