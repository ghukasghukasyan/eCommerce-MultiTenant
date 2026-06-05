using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Entities.Products;

namespace eCommerce.Domain.Interfaces.Categories
{
    public interface ICategoryRepository
    {
        void Deactivate(Category category);   
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId);
        Task<Category> GetByIdAsync(Guid id);
        Task<IEnumerable<Category>> GetAllAsync();
    }
}
