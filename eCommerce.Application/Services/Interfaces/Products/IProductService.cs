using eCommerce.Application.DTOs.Products;
using eCommerce.Application.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.Services.Interfaces.Products
{
    public interface IProductService
    {
        Task<ServiceResponse<Guid>> CreateAsync(CreateProductDTO dto);
        Task<ServiceResponse> UpdateAsync(UpdateProductDTO dto);
        Task<PagedResult<GetProductDTO>> GetAllAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<GetProductDTO>> GetRecentAsync(int days);
        Task<ServiceResponse> DeleteAsync(Guid id);
        Task<ServiceResponse> SetBestSellerAsync(Guid productId, bool value);
        Task<GetProductDTO> GetByIdAsync(Guid productId);
        Task<PagedResult<GetProductDTO>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20);
        Task<IEnumerable<GetProductDTO>> SearchAsync(string term);
        Task<ServiceResponse> RemoveImageAsync(Guid productId, string imageUrl);
        Task<ServiceResponse<string>> UploadImagesAsync(Guid productId, IFormFile file);
        Task<ServiceResponse> UpdateImagePositionAsync(Guid productId, string imageUrl, string objectPosition);
    }
}
