using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Products
{
    public class ProductVariantRepository(ECommerceContext context)
    : IVariantRepository
    {
        public async Task AddAsync(ProductVariant variant)
            => await context.ProductVariants.AddAsync(variant);

        public async Task<List<ProductVariant>> GetByProductIdAsync(Guid productId)
        {
            return await context.ProductVariants
                .Include(v => v.AttributeValues)
                .ThenInclude(va => va.VariantAttribute)
                .Where(v => v.ProductId == productId)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ProductVariant> variants)
       => await context.ProductVariants.AddRangeAsync(variants);

        public async Task DeleteByProductIdAsync(Guid productId)
        {
            var variants = await context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Include(v => v.AttributeValues)
                .ToListAsync();

            context.VariantAttributeValues.RemoveRange(
                variants.SelectMany(v => v.AttributeValues));

            context.ProductVariants.RemoveRange(variants);
        }

        public async Task<ProductVariant> GetByIdAsync(Guid variantId)
        {
            return await context.ProductVariants
                .Include(v => v.AttributeValues)
                    .ThenInclude(av => av.VariantAttribute)
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }

        public async Task<bool> DecrementStockAsync(Guid variantId, int quantity)
        {
            int rows = await context.ProductVariants
                .Where(v => v.Id == variantId && v.StockQuantity >= quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, v => v.StockQuantity - quantity));
            return rows > 0;
        }

        public async Task IncrementStockAsync(Guid variantId, int quantity)
        {
            await context.ProductVariants
                .Where(v => v.Id == variantId)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, v => v.StockQuantity + quantity));
        }
    }
}
