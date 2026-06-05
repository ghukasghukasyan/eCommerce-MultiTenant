using AutoMapper;
using eCommerce.Application.DTOs.Products;
using eCommerce.Application.Services.Implementations.Products;
using eCommerce.Application.Services.Interfaces.Files;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using Microsoft.AspNetCore.Http;
using Moq;
using static eCommerce.Domain.Enums.Types;

namespace eCommerce.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepo   = new();
    private readonly Mock<IVariantRepository> _variantRepo   = new();
    private readonly Mock<IFileService>       _fileService   = new();
    private readonly Mock<IMapper>            _mapper        = new();

    private ProductService CreateService() =>
        new(_productRepo.Object, _variantRepo.Object, _fileService.Object, _mapper.Object);

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Product ActiveProduct(string name = "Shirt", bool hasVariants = false) => new()
    {
        Id        = Guid.NewGuid(),
        Name      = name,
        BasePrice = 10m,
        HasVariants = hasVariants,
        Variants  = [ActiveVariant(1000m, 5)],
        Images    = []
    };

    private static ProductVariant ActiveVariant(decimal price = 1000m, int stock = 10) => new()
    {
        Id             = Guid.NewGuid(),
        Price          = price,
        StockQuantity  = stock,
        IsActive       = true,
        AttributeValues = []
    };

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsProductAndDefaultVariant_ReturnsId()
    {
        Product? savedProduct  = null;
        ProductVariant? savedVariant = null;

        _productRepo.Setup(x => x.CreateAsync(It.IsAny<Product>()))
            .Callback<Product>(p => savedProduct = p);
        _variantRepo.Setup(x => x.AddAsync(It.IsAny<ProductVariant>()))
            .Callback<ProductVariant>(v => savedVariant = v);

        var dto = new CreateProductDTO
        {
            Name         = "Blue Tee",
            Description  = "Desc",
            CategoryId   = Guid.NewGuid(),
            InitialPrice = 2500m,
            InitialStock = 8,
            HasVariants  = false
        };

        var result = await CreateService().CreateAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("Product created", result.Message);

        Assert.NotNull(savedProduct);
        Assert.Equal("Blue Tee", savedProduct!.Name);
        Assert.Equal(2500m, savedProduct.BasePrice);
        Assert.True(savedProduct.IsPublished);

        Assert.NotNull(savedVariant);
        Assert.Equal(2500m, savedVariant!.Price);
        Assert.Equal(8, savedVariant.StockQuantity);
        Assert.True(savedVariant.IsActive);

        _productRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenProductNotFound()
    {
        _productRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await CreateService().UpdateAsync(new UpdateProductDTO { Id = Guid.NewGuid(), Name = "X" });

        Assert.False(result.Success);
        Assert.Equal("Product not found", result.Message);
        _productRepo.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenNoVariants_AndSimpleProduct()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Old", HasVariants = false, Variants = [] };
        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await CreateService().UpdateAsync(new UpdateProductDTO { Id = product.Id, Name = "New", Price = 100, Stock = 5 });

        Assert.False(result.Success);
        Assert.Equal("Product has no variants", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProductAndVariant_WhenSimpleProduct()
    {
        var variant = ActiveVariant(100m, 3);
        var product = new Product { Id = Guid.NewGuid(), Name = "Old", HasVariants = false, Variants = [variant], Images = [] };
        var catId   = Guid.NewGuid();

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await CreateService().UpdateAsync(new UpdateProductDTO
        {
            Id          = product.Id,
            Name        = "New Name",
            Description = "New Desc",
            CategoryId  = catId,
            Price       = 9999m,
            Stock       = 50
        });

        Assert.True(result.Success);
        Assert.Equal("New Name", product.Name);
        Assert.Equal("New Desc", product.Description);
        Assert.Equal(catId, product.CategoryId);
        Assert.Equal(9999m, variant.Price);
        Assert.Equal(50, variant.StockQuantity);
        _productRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotModifyVariants_WhenProductHasVariants()
    {
        var variant = ActiveVariant(500m, 10);
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Old", HasVariants = true,
            Variants = [variant], Images = []
        };

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await CreateService().UpdateAsync(new UpdateProductDTO
        {
            Id = product.Id, Name = "New Name", Price = 9999m, Stock = 99
        });

        Assert.True(result.Success);
        Assert.Equal("New Name", product.Name);
        Assert.Equal(500m, variant.Price);   // unchanged — managed via VariantController
        Assert.Equal(10, variant.StockQuantity); // unchanged
        _productRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenProductNotFound()
    {
        _productRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await CreateService().DeleteAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Product not found", result.Message);
        _fileService.Verify(x => x.DeleteProductFolder(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesProduct_AndDeletesFolder()
    {
        var product = ActiveProduct();
        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await CreateService().DeleteAsync(product.Id);

        Assert.True(result.Success);
        Assert.True(product.IsDeleted);
        _productRepo.Verify(x => x.UpdateAsync(product), Times.Once);
        _productRepo.Verify(x => x.SaveAsync(), Times.Once);
        _fileService.Verify(x => x.DeleteProductFolder(product.Id), Times.Once);
    }

    // ── SetBestSellerAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SetBestSellerAsync_ReturnsFailure_WhenNotFound()
    {
        _productRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await CreateService().SetBestSellerAsync(Guid.NewGuid(), true);

        Assert.False(result.Success);
        Assert.Equal("Product not found", result.Message);
    }

    [Fact]
    public async Task SetBestSellerAsync_TogglesFlag_WhenFound()
    {
        var product = ActiveProduct();
        Assert.False(product.IsBestSeller);

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        await CreateService().SetBestSellerAsync(product.Id, true);

        Assert.True(product.IsBestSeller);
        _productRepo.Verify(x => x.UpdateAsync(product), Times.Once);
        _productRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProductNotFound()
    {
        _productRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await CreateService().GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoActiveVariants()
    {
        var product = new Product
        {
            Id       = Guid.NewGuid(),
            Name     = "Ghost",
            Variants = [new ProductVariant { IsActive = false, AttributeValues = [] }],
            Images   = []
        };

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await CreateService().GetByIdAsync(product.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_MapsPriceAsMinActiveVariantPrice()
    {
        var product = new Product
        {
            Id       = Guid.NewGuid(),
            Name     = "Multi",
            Variants =
            [
                new ProductVariant { Id = Guid.NewGuid(), Price = 500m, StockQuantity = 2, IsActive = true, AttributeValues = [] },
                new ProductVariant { Id = Guid.NewGuid(), Price = 300m, StockQuantity = 3, IsActive = true, AttributeValues = [] },
                new ProductVariant { Id = Guid.NewGuid(), Price = 100m, StockQuantity = 1, IsActive = false, AttributeValues = [] }
            ],
            Images = []
        };

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await CreateService().GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(300m, result!.Price);   // min of active: 500, 300 (100 is inactive)
        Assert.Equal(5, result.Stock);        // sum of active: 2 + 3
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenTermIsWhitespace()
    {
        var result = await CreateService().SearchAsync("   ");

        Assert.Empty(result);
        _productRepo.Verify(x => x.SearchAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenTermIsNull()
    {
        var result = await CreateService().SearchAsync(null!);

        Assert.Empty(result);
        _productRepo.Verify(x => x.SearchAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_DelegatesToRepo_WithTrimmedTerm()
    {
        var product = ActiveProduct("Jeans");

        _productRepo.Setup(x => x.SearchAsync("jeans")).ReturnsAsync([product]);

        var result = (await CreateService().SearchAsync("jeans")).ToList();

        Assert.Single(result);
        Assert.Equal("Jeans", result[0].Name);
    }

    // ── UploadImagesAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UploadImagesAsync_ReturnsFailure_WhenProductNotFound()
    {
        _productRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await CreateService().UploadImagesAsync(Guid.NewGuid(), Mock.Of<IFormFile>());

        Assert.False(result.Success);
        Assert.Equal("Product not found", result.Message);
        _fileService.Verify(x => x.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<FileEntityType>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UploadImagesAsync_SetsIsMain_WhenProductHasNoImages()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "X", Variants = [], Images = [] };
        var file    = Mock.Of<IFormFile>();

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _fileService.Setup(x => x.SaveImageAsync(file, FileEntityType.Product, product.Id))
            .ReturnsAsync("/imgs/photo.jpg");

        ProductImage? savedImage = null;
        _productRepo.Setup(x => x.AddImagesAsync(It.IsAny<IEnumerable<ProductImage>>()))
            .Callback<IEnumerable<ProductImage>>(imgs => savedImage = imgs.First());

        var result = await CreateService().UploadImagesAsync(product.Id, file);

        Assert.True(result.Success);
        Assert.Equal("/imgs/photo.jpg", result.Data);
        Assert.NotNull(savedImage);
        Assert.True(savedImage!.IsMain);
        Assert.Equal(0, savedImage.DisplayOrder);
    }

    [Fact]
    public async Task UploadImagesAsync_DoesNotSetIsMain_WhenImagesAlreadyExist()
    {
        var existing = new ProductImage { ImageUrl = "/old.jpg", IsMain = true };
        var product  = new Product { Id = Guid.NewGuid(), Name = "X", Variants = [], Images = [existing] };
        var file     = Mock.Of<IFormFile>();

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _fileService.Setup(x => x.SaveImageAsync(file, FileEntityType.Product, product.Id))
            .ReturnsAsync("/imgs/new.jpg");

        ProductImage? savedImage = null;
        _productRepo.Setup(x => x.AddImagesAsync(It.IsAny<IEnumerable<ProductImage>>()))
            .Callback<IEnumerable<ProductImage>>(imgs => savedImage = imgs.First());

        await CreateService().UploadImagesAsync(product.Id, file);

        Assert.NotNull(savedImage);
        Assert.False(savedImage!.IsMain);
        Assert.Equal(1, savedImage.DisplayOrder);
    }

    [Fact]
    public async Task UploadImagesAsync_CleansUpFile_WhenSaveAsyncFails()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "X", Variants = [], Images = [] };
        var file    = Mock.Of<IFormFile>();

        _productRepo.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _fileService.Setup(x => x.SaveImageAsync(file, FileEntityType.Product, product.Id))
            .ReturnsAsync("/imgs/fail.jpg");
        _productRepo.Setup(x => x.SaveAsync()).ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<Exception>(() => CreateService().UploadImagesAsync(product.Id, file));

        _fileService.Verify(x => x.DeleteProductImage("/imgs/fail.jpg"), Times.Once);
    }

    // ── RemoveImageAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveImageAsync_ReturnsFailure_WhenImageNotFound()
    {
        _productRepo.Setup(x => x.GetImagesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<ProductImage>());

        var result = await CreateService().RemoveImageAsync(Guid.NewGuid(), "/missing.jpg");

        Assert.False(result.Success);
        Assert.Equal("Image not found", result.Message);
        _productRepo.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoveImageAsync_RemovesImageAndDeletesFile_WhenFound()
    {
        var productId = Guid.NewGuid();
        var image     = new ProductImage { ProductId = productId, ImageUrl = "/to-delete.jpg" };

        _productRepo.Setup(x => x.GetImagesAsync(productId)).ReturnsAsync([image]);

        var result = await CreateService().RemoveImageAsync(productId, "/to-delete.jpg");

        Assert.True(result.Success);
        _productRepo.Verify(x => x.RemoveImages(It.Is<IEnumerable<ProductImage>>(imgs => imgs.Contains(image))), Times.Once);
        _fileService.Verify(x => x.DeleteProductImage("/to-delete.jpg"), Times.Once);
        _productRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveImageAsync_DoesNotDeleteFile_WhenSaveAsyncFails()
    {
        var productId = Guid.NewGuid();
        var image     = new ProductImage { ProductId = productId, ImageUrl = "/fail.jpg" };

        _productRepo.Setup(x => x.GetImagesAsync(productId)).ReturnsAsync([image]);
        _productRepo.Setup(x => x.SaveAsync()).ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<Exception>(() => CreateService().RemoveImageAsync(productId, "/fail.jpg"));

        _fileService.Verify(x => x.DeleteProductImage(It.IsAny<string>()), Times.Never);
    }
}
