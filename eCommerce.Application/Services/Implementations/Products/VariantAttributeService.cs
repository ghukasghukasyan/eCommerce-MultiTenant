using eCommerce.Application.DTOs.Products.VariantAttributes;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Products;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;

namespace eCommerce.Application.Services.Implementations.Products
{
    public class VariantAttributeService(IVariantAttributeRepository repo) : IVariantAttributeService
    {
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateVariantAttributeDTO dto)
        {
            var attr = new VariantAttribute
            {
                Name = dto.Name
            };

            await repo.AddAsync(attr);
            await repo.SaveAsync();

            return new ServiceResponse<Guid>(true, attr.Id, "Attribute created");
        }

        public async Task<List<VariantAttributeDTO>> GetAllAsync()
        {
            var attrs = await repo.GetAllAsync();
            return attrs.Select(a => new VariantAttributeDTO
            {
                Id = a.Id,
                Name = a.Name
            }).ToList();
        }
    }
}
