using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Categories;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Categories
{
    public class CategoryRepository(ECommerceContext context) : ICategoryRepository
    {
        public void Deactivate(Category category)
        {
            category.IsActive = false;
            context.Categories.Update(category);
        }

        public async Task<Category> GetByIdAsync(Guid id)
        {
            return await context.Categories
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await context.Categories
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId)
        {
            List<Product> products = await context.Products
                .Include(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.Variants)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsPublished)
                .AsNoTracking()
                .ToListAsync();

            return products.Count > 0 ? products : [];
        }
    }
}
