using ClientLibrary.Models.Categories;
using ClientLibrary.Models.Products;
using ClientLibrary.Models.Responses;

namespace ClientLibrary.Services.Interface
{
    public interface ICategoryService
    {
        Task<ServiceResponse<Guid>> AddAsync(CreateCategoryDTO category);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateCategoryDTO category);
        Task<ServiceResponse<Guid>> DeleteAsync(Guid id);
        Task<IEnumerable<GetCategoryDTO>> GetAllAsync();
        Task<GetCategoryDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<GetProductDTO>> GetProductsByCategory(Guid categoryId);
        Task PreloadProductsForCategoriesAsync(IEnumerable<Guid> categoryIds);
    }
}
