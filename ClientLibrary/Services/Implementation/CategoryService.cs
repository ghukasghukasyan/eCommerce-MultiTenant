using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Categories;
using ClientLibrary.Models.Products;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class CategoryService(IHttpClientHelper httpClient, IApiCallHelper apiHelper, IMemoryCache cache) : ICategoryService
    {
        private const string CategoriesCacheKey = "categories:all";
        private static readonly TimeSpan CategoriesCacheDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan CategoryProductsCacheDuration = TimeSpan.FromMinutes(10);
        private static readonly ConcurrentDictionary<string, byte> CategoryProductsCacheKeys = new();

        public async Task<ServiceResponse<Guid>> AddAsync(CreateCategoryDTO category)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Category.Create,
                Type = ApiCallType.Post,
                Client = client,
                Id = null!,
                Model = category,
            };

            var result = await apiHelper.ApiCallTypeCall<CreateCategoryDTO>(apiCall);
            cache.Remove(CategoriesCacheKey);
            RemoveCategoryProductsCaches(cache);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateCategoryDTO category)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Category.Update,
                Type = ApiCallType.Update,
                Client = client,
                Id = null!,
                Model = category,
            };

            var result = await apiHelper.ApiCallTypeCall<UpdateCategoryDTO>(apiCall);
            cache.Remove(CategoriesCacheKey);
            RemoveCategoryProductsCaches(cache);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> DeleteAsync(Guid id)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Category.Delete,
                Type = ApiCallType.Delete,
                Client = client,
                Model = null!,
            };

            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            cache.Remove(CategoriesCacheKey);
            cache.Remove(BuildCategoryProductsKey(id));
            RemoveCategoryProductsCaches(cache);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<IEnumerable<GetCategoryDTO>> GetAllAsync()
        {
            if (cache.TryGetValue<IEnumerable<GetCategoryDTO>>(CategoriesCacheKey, out var cachedCategories)
                && cachedCategories is not null)
            {
                return cachedCategories;
            }

            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Category.GetAll,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
            {
                var categories = await apiHelper.GetServiceResponse<IEnumerable<GetCategoryDTO>>(result);
                var cachedValue = categories.ToList();
                cache.Set(CategoriesCacheKey, cachedValue, CategoriesCacheDuration);
                return cachedValue;
            }

            return [];
        }
        public async Task<GetCategoryDTO> GetByIdAsync(Guid id)
        {
            if (cache.TryGetValue<IEnumerable<GetCategoryDTO>>(CategoriesCacheKey, out var cachedCategories)
                && cachedCategories is not null)
            {
                var fromCache = cachedCategories.FirstOrDefault(c => c.Id == id);
                if (fromCache is not null)
                    return fromCache;
            }

            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Category.Get,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };
            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<GetCategoryDTO>(result);
            else
                return null!;
        }
        public async Task<IEnumerable<GetProductDTO>> GetProductsByCategory(Guid categoryId)
        {
            var cacheKey = BuildCategoryProductsKey(categoryId);
            if (cache.TryGetValue<IEnumerable<GetProductDTO>>(cacheKey, out var cachedProducts)
                && cachedProducts is not null)
            {
                return cachedProducts;
            }

            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Product.GetProductsByCategory,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };
            apiCall.ToString(categoryId);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
            {
                var paged = await apiHelper.GetServiceResponse<PagedResult<GetProductDTO>>(result);
                var cachedValue = paged.Items.ToList();
                cache.Set(cacheKey, cachedValue, CategoryProductsCacheDuration);
                CategoryProductsCacheKeys.TryAdd(cacheKey, 0);
                return cachedValue;
            }

            return [];
        }

        public async Task PreloadProductsForCategoriesAsync(IEnumerable<Guid> categoryIds)
        {
            foreach (var categoryId in categoryIds.Take(3))
            {
                await GetProductsByCategory(categoryId);
            }
        }

        private static string BuildCategoryProductsKey(Guid categoryId) => $"products:category:{categoryId}";

        private static void RemoveCategoryProductsCaches(IMemoryCache cacheInstance)
        {
            foreach (var key in CategoryProductsCacheKeys.Keys)
            {
                cacheInstance.Remove(key);
                CategoryProductsCacheKeys.TryRemove(key, out _);
            }
        }
    }
}
