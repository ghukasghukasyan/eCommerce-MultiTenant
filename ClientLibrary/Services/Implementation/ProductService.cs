using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Products;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Caching.Memory;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class ProductService(IHttpClientHelper httpClient, IApiCallHelper apiHelper, IMemoryCache cache) : IProductService
    {
        private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(10);
        private const string AllProductsCacheKey = "products:all";
        private const string RecentProductsCacheKey = "products:recent";

        public async Task<ServiceResponse<Guid>> AddAsync(CreateProductDTO product)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Product.Create,
                Type = ApiCallType.Post,
                Client = client,
                Id = null!,
                Model = product,
            };

            var result = await apiHelper.ApiCallTypeCall<CreateProductDTO>(apiCall);
            cache.Remove(AllProductsCacheKey);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateProductDTO category)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Product.Update,
                Type = ApiCallType.Update,
                Client = client,
                Id = null!,
                Model = category,
            };

            var result = await apiHelper.ApiCallTypeCall<UpdateProductDTO>(apiCall);
            cache.Remove(AllProductsCacheKey);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> DeleteAsync(Guid id)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Product.Delete,
                Type = ApiCallType.Delete,
                Client = client,
                Model = null!,
            };

            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            cache.Remove(AllProductsCacheKey);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<string> UploadImageAsync(Guid productId, IBrowserFile file)
        {
            var client = await httpClient.GetPrivateClientAsync();

            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(file.OpenReadStream(20_000_000));
            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "File", file.Name);
            content.Add(new StringContent(productId.ToString()), "ProductId");

            var response = await client.PostAsync(Product.Upload, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return (await response.Content.ReadAsStringAsync()).Trim('"');
        }
        public async Task<ServiceResponse> DeleteImageAsync(Guid productId, string imageUrl)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var encodedUrl = Uri.EscapeDataString(imageUrl);

            var response = await client.DeleteAsync(
                $"{Product.DeleteImage}?productId={productId}&imageUrl={encodedUrl}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new ServiceResponse(false, error);
            }

            return new ServiceResponse(true, "Image deleted successfully.");
        }

        public async Task<ServiceResponse> UpdateImagePositionAsync(Guid productId, string imageUrl, string objectPosition)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                productId,
                imageUrl,
                objectPosition
            });
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(Product.UpdateImagePosition, content);

            if (!response.IsSuccessStatusCode)
                return new ServiceResponse(false, await response.Content.ReadAsStringAsync());

            return new ServiceResponse(true, "Position updated.");
        }

        public async Task<PagedResult<GetProductDTO>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            var cacheKey = $"{AllProductsCacheKey}:{page}:{pageSize}";
            if (cache.TryGetValue<PagedResult<GetProductDTO>>(cacheKey, out var cached) && cached is not null)
                return cached;

            var client = await httpClient.GetPublicClientAsync();
            var response = await client.GetAsync($"{Product.GetAll}?page={page}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode)
                return new PagedResult<GetProductDTO> { Items = [], TotalCount = 0 };

            var result = await apiHelper.GetServiceResponse<PagedResult<GetProductDTO>>(response);
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        public async Task<IEnumerable<GetProductDTO>> GetRecentAsync()
        {
            if (cache.TryGetValue<IEnumerable<GetProductDTO>>(RecentProductsCacheKey, out var cached) && cached is not null)
                return cached;

            var client = await httpClient.GetPublicClientAsync();
            var response = await client.GetAsync(Product.GetRecent);
            if (!response.IsSuccessStatusCode)
                return [];

            var products = await apiHelper.GetServiceResponse<IEnumerable<GetProductDTO>>(response);
            var list = products.ToList();
            cache.Set(RecentProductsCacheKey, list, TimeSpan.FromMinutes(5));
            return list;
        }

        public async Task<IEnumerable<GetProductDTO>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return [];

            var normalizedTerm = searchTerm.Trim().ToLowerInvariant();
            var cacheKey = $"products:search:{normalizedTerm}";
            if (cache.TryGetValue<IEnumerable<GetProductDTO>>(cacheKey, out var cachedProducts)
                && cachedProducts is not null)
            {
                return cachedProducts;
            }

            var client = await httpClient.GetPublicClientAsync();
            var encodedTerm = Uri.EscapeDataString(searchTerm.Trim());
            var response = await client.GetAsync($"{Product.Search}?term={encodedTerm}");
            if (!response.IsSuccessStatusCode)
                return [];

            var products = await apiHelper.GetServiceResponse<IEnumerable<GetProductDTO>>(response);
            var cachedValue = products.ToList();
            cache.Set(cacheKey, cachedValue, SearchCacheDuration);
            return cachedValue;
        }
        public async Task<GetProductDTO> GetByIdAsync(Guid id)
        {
            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Product.Get,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };
            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<GetProductDTO>(result);
            return null!;
        }

        public async Task<ServiceResponse<Guid>> SetBestSellerAsync(Guid productId, bool isBestSeller)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = $"product/bestSeller/{productId}",
                Type = ApiCallType.Update,
                Client = client,
                Id = null!,
                Model = isBestSeller,
            };

            var result = await apiHelper.ApiCallTypeCall<bool>(apiCall);

            return result == null
                ? apiHelper.ConnectionError()
                : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
    }
}
