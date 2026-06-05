using eCommerce.Application.DTOs.Products.Variants;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Products
{
    public interface IVariantService
    {
        Task<ServiceResponse> GenerateAsync(GenerateVariantsDTO variantDTO);
        Task<List<VariantDTO>> GetByProductIdAsync(Guid productId);
        Task<ServiceResponse> UpdateVariantAsync(UpdateVariantDTO variantDTO);
    }
}
