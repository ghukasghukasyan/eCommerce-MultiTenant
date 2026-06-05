using eCommerce.Domain.Entities.Products;

namespace eCommerce.Domain.Interfaces.Products
{

    public interface IProductRepository
    {
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task AddImagesAsync(IEnumerable<ProductImage> images);
        void RemoveImages(IEnumerable<ProductImage> images);
        Task SaveAsync();
        Task<IEnumerable<Product>> GetAllAsync();
        Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int skip, int take);
        Task<IEnumerable<Product>> GetRecentAsync(int days);
        Task<Product> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
        Task<(IEnumerable<Product> Items, int Total)> GetPagedByCategoryAsync(Guid categoryId, int skip, int take);
        Task<IEnumerable<Product>> SearchAsync(string term);
        Task<List<ProductImage>> GetImagesAsync(Guid productId);
        Task<ProductImage?> GetImageByUrlAsync(Guid productId, string imageUrl);
        Task<int> CountAsync();
    }
}
