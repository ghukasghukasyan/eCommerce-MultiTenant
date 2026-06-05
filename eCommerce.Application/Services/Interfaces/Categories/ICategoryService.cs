using eCommerce.Application.DTOs.Categories;
using eCommerce.Application.DTOs.Products;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<GetCategoryDTO>> GetAllAsync();
        Task<GetCategoryDTO> GetByIdAsync(Guid id);
        Task<ServiceResponse> CreateAsync(CreateCategoryDTO categoryDTO);
        Task<ServiceResponse> UpdateAsync(UpdateCategoryDTO cayegorytDTO);
        Task<ServiceResponse> DeleteAsync(Guid id);
        Task<IEnumerable<GetProductDTO>> GetProductsByCategory(Guid id);
    }
}
