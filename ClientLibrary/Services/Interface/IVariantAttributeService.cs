using ClientLibrary.Models.Responses;
using ClientLibrary.Models.Variants;

namespace ClientLibrary.Services.Interface
{
    public interface IVariantAttributeService
    {
        Task<List<VariantAttributeDTO>> GetAllAsync();
        Task<ServiceResponse<Guid>> CreateAsync(string name);
    }
}
