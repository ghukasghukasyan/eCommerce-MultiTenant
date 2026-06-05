using AutoMapper;
using eCommerce.Application.DTOs.Products;
using eCommerce.Application.DTOs.Products.Variants;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Files;
using eCommerce.Application.Services.Interfaces.Products;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using Microsoft.AspNetCore.Http;
using static eCommerce.Domain.Enums.Types;

namespace eCommerce.Application.Services.Implementations.Products
{
    public class ProductService(IProductRepository productRepository, IVariantRepository variantRepository, IFileService fileService, IMapper mapper) : IProductService
    {
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateProductDTO dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                BasePrice = dto.InitialPrice,
                HasVariants = dto.HasVariants,
                IsPublished = true
            };

            await productRepository.CreateAsync(product);

            var defaultVariant = new ProductVariant
            {
                Product = product,
                Price = dto.InitialPrice,
                StockQuantity = dto.InitialStock,
                Sku = GenerateSku(dto.Name),
                IsActive = true
            };

            await variantRepository.AddAsync(defaultVariant);
            await productRepository.SaveAsync();

            return new ServiceResponse<Guid>(true, product.Id, "Product created");
        }

        public async Task<ServiceResponse> UpdateAsync(UpdateProductDTO dto)
        {
            var product = await productRepository.GetByIdAsync(dto.Id);
            if (product == null)
                return new ServiceResponse(false, "Product not found");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;

            // Multi-variant products manage price/stock per-variant via VariantController
            if (!product.HasVariants)
            {
                var variant = product.Variants.FirstOrDefault();
                if (variant == null)
                    return new ServiceResponse(false, "Product has no variants");

                variant.Price = dto.Price;
                variant.StockQuantity = dto.Stock;
            }

            await productRepository.SaveAsync();
            return new ServiceResponse(true, "Updated");
        }

        public async Task<ServiceResponse> DeleteAsync(Guid id)
        {
            var product = await productRepository.GetByIdAsync(id);
            if (product == null)
                return new ServiceResponse(false, "Product not found");

            product.SoftDelete();
            await productRepository.UpdateAsync(product);
            await productRepository.SaveAsync();
            fileService.DeleteProductFolder(id);
            return new ServiceResponse(true, "Product deleted");
        }

        public async Task<PagedResult<GetProductDTO>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            var (items, total) = await productRepository.GetPagedAsync((page - 1) * pageSize, pageSize);
            return new PagedResult<GetProductDTO>
            {
                Items = MapToGetProductDtos(items).ToList(),
                TotalCount = total
            };
        }

        public async Task<GetProductDTO> GetByIdAsync(Guid productId)
        {
            var product = await productRepository.GetByIdAsync(productId);
            return product == null ? null : MapSingle(product);
        }

        public async Task<ServiceResponse> SetBestSellerAsync(Guid productId, bool bestSeller)
        {
            var product = await productRepository.GetByIdAsync(productId);
            if (product == null)
                return new ServiceResponse(false, "Product not found");

            product.SetBestSeller(bestSeller);
            await productRepository.UpdateAsync(product);
            await productRepository.SaveAsync();
            return new ServiceResponse(true, "Product updated");
        }

        public async Task<IEnumerable<GetProductDTO>> GetRecentAsync(int days)
        {
            var products = await productRepository.GetRecentAsync(days);
            return MapToGetProductDtos(products);
        }

        public async Task<PagedResult<GetProductDTO>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20)
        {
            var (items, total) = await productRepository.GetPagedByCategoryAsync(categoryId, (page - 1) * pageSize, pageSize);
            return new PagedResult<GetProductDTO>
            {
                Items = MapToGetProductDtos(items).ToList(),
                TotalCount = total
            };
        }

        public async Task<IEnumerable<GetProductDTO>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return [];

            var products = await productRepository.SearchAsync(term);
            return MapToGetProductDtos(products);
        }

        public async Task<ServiceResponse> UpdateImagePositionAsync(Guid productId, string imageUrl, string objectPosition)
        {
            var image = await productRepository.GetImageByUrlAsync(productId, imageUrl);
            if (image == null)
                return new ServiceResponse(false, "Image not found");

            image.ObjectPosition = objectPosition;
            await productRepository.SaveAsync();
            return new ServiceResponse(true, "Position updated");
        }

        public async Task<ServiceResponse> RemoveImageAsync(Guid productId, string imageUrl)
        {
            var images = await productRepository.GetImagesAsync(productId);
            var image = images.FirstOrDefault(i => i.ImageUrl == imageUrl);

            if (image == null)
                return new ServiceResponse(false, "Image not found");

            productRepository.RemoveImages(new[] { image });
            await productRepository.SaveAsync();
            fileService.DeleteProductImage(imageUrl);

            return new ServiceResponse(true, "Image removed");
        }

        public async Task<ServiceResponse<string>> UploadImagesAsync(
     Guid productId,
     IFormFile file)
        {
            var product = await productRepository.GetByIdAsync(productId);

            if (product == null)
                return new ServiceResponse<string>(false, string.Empty, "Product not found");

            var imagePath = await fileService.SaveImageAsync(
                file,
                FileEntityType.Product,
                productId);

            var image = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imagePath,
                IsMain = !product.Images.Any(),
                DisplayOrder = product.Images.Count
            };

            await productRepository.AddImagesAsync(new List<ProductImage> { image });
            try
            {
                await productRepository.SaveAsync();
            }
            catch
            {
                fileService.DeleteProductImage(imagePath);
                throw;
            }

            return new ServiceResponse<string>(true, imagePath, "Saved correctly");
        }

        private static string GenerateSku(string name)
        {
            var prefix = name[..Math.Min(3, name.Length)].ToUpper();
            var random = Random.Shared.Next(1000, 9999);
            return $"{prefix}-{random}";
        }

        private static GetProductDTO MapSingle(Product p)
        {
            var activeVariants = p.Variants.Where(v => v.IsActive).ToList();
            var defaultVariant = activeVariants.FirstOrDefault();
            if (defaultVariant == null) return null;

            return new GetProductDTO
            {
                Id = p.Id,
                DefaultVariantId = defaultVariant.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                IsBestSeller = p.IsBestSeller,
                CreatedAt = p.CreatedAt,
                Price = activeVariants.Min(v => v.Price),
                Stock = activeVariants.Sum(v => v.StockQuantity),
                Images = [.. p.Images.Select(i => new ProductImageDTO
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder
                })],
                Variants = [.. p.Variants.Select(v => new VariantDTO
                {
                    VariantId = v.Id,
                    Attributes = v.AttributeValues
                        .ToDictionary(av => av.VariantAttribute.Name, av => av.Value),
                    Price = v.Price,
                    Stock = v.StockQuantity,
                    IsActive = v.IsActive
                })]
            };
        }

        private static IEnumerable<GetProductDTO> MapToGetProductDtos(IEnumerable<Product> products) =>
            products.Select(MapSingle).Where(d => d != null).Select(d => d!);
    }
}
