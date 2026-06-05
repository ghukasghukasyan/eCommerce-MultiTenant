using eCommerce.Application.DTOs.Products.VariantAttributes;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Products
{
    public interface IVariantAttributeService
    {
        Task<ServiceResponse<Guid>> CreateAsync(CreateVariantAttributeDTO dto);
        Task<List<VariantAttributeDTO>> GetAllAsync();
    }
}
