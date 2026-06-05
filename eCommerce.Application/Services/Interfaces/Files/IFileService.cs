using Microsoft.AspNetCore.Http;
using static eCommerce.Domain.Enums.Types;

namespace eCommerce.Application.Services.Interfaces.Files
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file, FileEntityType entityType, Guid entityId);
        void DeleteProductImage(string imageUrl);
        void DeleteProductFolder(Guid productId);
    }
}
