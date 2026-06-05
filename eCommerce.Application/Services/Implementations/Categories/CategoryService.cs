using AutoMapper;
using eCommerce.Application.DTOs.Categories;
using eCommerce.Application.DTOs.Products;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Categories;
using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Interfaces;
using eCommerce.Domain.Interfaces.Categories;
using Microsoft.Extensions.Caching.Memory;

namespace eCommerce.Application.Services.Implementations.Categories
{
    public class CategoryService(IGeneric<Category> genericCategoryRepository, IMapper mapper, ICategoryRepository categoryRepository, IMemoryCache cache) : ICategoryService
    {
        private const string CategoriesCacheKey = "categories:all";
        public async Task<ServiceResponse> CreateAsync(CreateCategoryDTO categoryDTO)
        {
            var category = new Category
            {
                Name = categoryDTO.Name,
                ParentCategoryId = categoryDTO.ParentCategoryId,
                DisplayOrder = categoryDTO.DisplayOrder
            };

            int result = await genericCategoryRepository.AddAsync(category);
            if (result > 0) cache.Remove(CategoriesCacheKey);
            return result > 0 ? new ServiceResponse(true, "Category added!") : new ServiceResponse(false, "Category failed to be added!");
        }
        public async Task<ServiceResponse> UpdateAsync(UpdateCategoryDTO categoryDTO)
        {
            var category = await genericCategoryRepository.GetByIdAsync(categoryDTO.Id);
            if (category == null)
                return new(false, "Category not found");

            if (categoryDTO.ParentCategoryId == categoryDTO.Id)
                return new(false, "Category cannot be its own parent");

            category.Name = categoryDTO.Name;
            category.ParentCategoryId = categoryDTO.ParentCategoryId;
            category.DisplayOrder = categoryDTO.DisplayOrder;
            category.IsActive = categoryDTO.IsActive;

            int result = await genericCategoryRepository.UpdateAsync(category);
            if (result > 0) cache.Remove(CategoriesCacheKey);
            return result > 0 ? new ServiceResponse(true, "Category updated!") : new ServiceResponse(false, "Category failed to be updated!");
        }

        public async Task<ServiceResponse> DeleteAsync(Guid id)
        {
            int result = await genericCategoryRepository.DeleteAsync(id);
            if (result > 0) cache.Remove(CategoriesCacheKey);
            return result > 0 ? new ServiceResponse(true, "Category deleted!") : new ServiceResponse(false, "Category not found or failed to be deleted!");
        }

        public async Task<IEnumerable<GetCategoryDTO>> GetAllAsync()
        {
            if (cache.TryGetValue(CategoriesCacheKey, out IEnumerable<GetCategoryDTO>? cached))
                return cached!;

            var result = await categoryRepository.GetAllAsync();
            if (!result.Any()) return [];

            var mapped = mapper.Map<IEnumerable<GetCategoryDTO>>(result);
            cache.Set(CategoriesCacheKey, mapped, TimeSpan.FromMinutes(5));
            return mapped;
        }

        public async Task<GetCategoryDTO> GetByIdAsync(Guid id)
        {
            var result = await categoryRepository.GetByIdAsync(id);
            return result == null ? new GetCategoryDTO() : mapper.Map<GetCategoryDTO>(result);
        }

        public async Task<IEnumerable<GetProductDTO>> GetProductsByCategory(Guid id)
        {
            var products = await categoryRepository.GetProductsByCategoryIdAsync(id);
            return !products.Any() ? [] : mapper.Map<IEnumerable<GetProductDTO>>(products);
        }

    }
}
