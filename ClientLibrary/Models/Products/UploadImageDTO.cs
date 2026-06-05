using Microsoft.AspNetCore.Http;

namespace ClientLibrary.Models.Products
{
    public class UploadImageDTO
    {
        public IFormFile File { get; set; }
        public Guid ProductId { get; set; }
    }
}
