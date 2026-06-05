using eCommerce.Application.Services.Interfaces.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using static eCommerce.Domain.Enums.Types;

namespace eCommerce.Infrastructure.Services
{
    public class FileService(IWebHostEnvironment env) : IFileService
    {
        private readonly IWebHostEnvironment _env = env;
        private const long MaxImageSize = 20 * 1024 * 1024;
        private const int MaxWidth = 1200;
        private const int WebpQuality = 82;
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

        public async Task<string> SaveImageAsync(
            IFormFile file,
            FileEntityType entityType,
            Guid entityId)
        {
            ValidateImage(file);

            var folder = Path.Combine(
                _env.WebRootPath,
                "uploads",
                entityType.ToString().ToLower(),
                entityId.ToString());

            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}.webp";
            var path = Path.Combine(folder, fileName);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            image.Mutate(x => x.AutoOrient());

            if (image.Width > MaxWidth)
                image.Mutate(x => x.Resize(MaxWidth, 0));

            await image.SaveAsWebpAsync(path, new WebpEncoder { Quality = WebpQuality });

            return $"/uploads/{entityType.ToString().ToLower()}/{entityId}/{fileName}";
        }

        public void DeleteProductImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var path = Path.Combine(_env.WebRootPath,
                imageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(path))
                File.Delete(path);
        }

        public void DeleteProductFolder(Guid productId)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads",
                FileEntityType.Product.ToString().ToLower(), productId.ToString());
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        private static void ValidateImage(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file), "File is required");

            if (file.Length == 0)
                throw new Exception("File is empty");

            if (file.Length > MaxImageSize)
                throw new Exception("Image size exceeds 20MB limit");

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new Exception($"Invalid image content type: {file.ContentType}");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new Exception($"Invalid image extension: {extension}");
        }
    }
}
