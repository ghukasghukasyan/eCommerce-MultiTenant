using eCommerce.Domain.Entities.Products;

namespace eCommerce.Domain.Interfaces.Products
{
    public interface IVariantRepository
    {
        Task AddAsync(ProductVariant variant);
        Task AddRangeAsync(IEnumerable<ProductVariant> variants);
        Task DeleteByProductIdAsync(Guid productId);
        Task<ProductVariant> GetByIdAsync(Guid variantId);
        Task<List<ProductVariant>> GetByProductIdAsync(Guid productId);
        Task<bool> DecrementStockAsync(Guid variantId, int quantity);
        Task IncrementStockAsync(Guid variantId, int quantity);
    }
}
