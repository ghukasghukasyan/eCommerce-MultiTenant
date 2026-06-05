using ClientLibrary.Models.Responses;
using ClientLibrary.Models.Variants;

namespace ClientLibrary.Services.Interface
{
    public interface IVariantService
    {
        Task<List<VariantDTO>> GetByProductIdAsync(Guid productId);
        Task<ServiceResponse> GenerateAsync(GenerateVariantsDTO dto);
        Task<ServiceResponse> UpdateVariantAsync(UpdateVariantDTO dto);
    }
}
