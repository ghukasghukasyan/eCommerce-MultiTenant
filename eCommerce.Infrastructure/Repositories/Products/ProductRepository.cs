using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Products
{
    public class ProductRepository(ECommerceContext context)
        : IProductRepository
    {
        public async Task CreateAsync(Product product)
            => await context.Products.AddAsync(product);

        public Task UpdateAsync(Product product)
        {
            context.Products.Update(product);
            return Task.CompletedTask;
        }

        public async Task<Product> GetByIdAsync(Guid id)
        {
            return await context.Products
                        .Include(p => p.Images)
                        .Include(p => p.Variants)
                            .ThenInclude(v => v.AttributeValues)
                                .ThenInclude(av => av.VariantAttribute)
                        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await context.Products
                .Include(p=> p.Images)
                .Include(p => p.Variants)
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int skip, int take)
        {
            var query = context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip).Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<(IEnumerable<Product> Items, int Total)> GetPagedByCategoryAsync(Guid categoryId, int skip, int take)
        {
            var query = context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsPublished && p.CategoryId == categoryId);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip).Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<Product>> GetRecentAsync(int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            return await context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsPublished && p.CreatedAt >= cutoff)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
        {
            return await context.Products
               .Include(p => p.Images)
               .Include(p => p.Variants)
               .AsNoTracking()
               .Where(p => !p.IsDeleted && p.IsPublished && p.CategoryId == categoryId)
               .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string term)
        {
            var pattern = $"%{term.Trim()}%";

            return await context.Products
               .Include(p => p.Images)
               .Include(p => p.Variants)
               .AsNoTracking()
               .Where(p => !p.IsDeleted && p.IsPublished &&
                           (EF.Functions.ILike(p.Name, pattern) ||
                            EF.Functions.ILike(p.Description ?? string.Empty, pattern)))
               .ToListAsync();
        }

        public async Task<List<ProductImage>> GetImagesAsync(Guid productId)
        => await context.ProductImages
            .Where(i => i.ProductId == productId)
            .ToListAsync();

        public async Task<ProductImage?> GetImageByUrlAsync(Guid productId, string imageUrl)
        => await context.ProductImages
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.ImageUrl == imageUrl);

        public async Task AddImagesAsync(IEnumerable<ProductImage> images)
            => await context.ProductImages.AddRangeAsync(images);

        public void RemoveImages(IEnumerable<ProductImage> images)
            => context.ProductImages.RemoveRange(images);

        public async Task SaveAsync()
            => await context.SaveChangesAsync();

        public async Task<int> CountAsync()
            => await context.Products.CountAsync(p => !p.IsDeleted);
    }
}
