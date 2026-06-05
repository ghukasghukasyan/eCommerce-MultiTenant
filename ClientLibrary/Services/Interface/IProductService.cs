using ClientLibrary.Models.Products;
using ClientLibrary.Models.Responses;
using Microsoft.AspNetCore.Components.Forms;

namespace ClientLibrary.Services.Interface
{
    public interface IProductService
    {
        Task<ServiceResponse<Guid>> AddAsync(CreateProductDTO product);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateProductDTO product);
        Task<ServiceResponse<Guid>> DeleteAsync(Guid id);
        Task<ServiceResponse> DeleteImageAsync(Guid productId, string imageUrl);
        Task<string> UploadImageAsync(Guid productId, IBrowserFile file);
        Task<ServiceResponse> UpdateImagePositionAsync(Guid productId, string imageUrl, string objectPosition);
        Task<PagedResult<GetProductDTO>> GetAllAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<GetProductDTO>> GetRecentAsync();
        Task<IEnumerable<GetProductDTO>> SearchAsync(string searchTerm);
        Task<GetProductDTO> GetByIdAsync(Guid id);
        Task<ServiceResponse<Guid>> SetBestSellerAsync(Guid productId, bool isBestSeller);
    }
}
