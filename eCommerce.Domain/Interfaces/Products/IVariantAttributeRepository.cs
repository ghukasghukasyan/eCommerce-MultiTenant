using eCommerce.Domain.Entities.Products;

namespace eCommerce.Domain.Interfaces.Products
{
    public interface IVariantAttributeRepository
    {
        Task AddAsync(VariantAttribute attribute);
        Task SaveAsync();   
        Task<List<VariantAttribute>> GetAllAsync();
    }
}
