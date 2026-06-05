using eCommerce.Application.DTOs.Products.Variants;
using eCommerce.Application.Services.Implementations.Products;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using Moq;

namespace eCommerce.Tests.Services;

public class VariantServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IVariantRepository> _variantRepository = new();

    private VariantService CreateService() => new(_productRepository.Object, _variantRepository.Object);

    [Fact]
    public async Task GenerateAsync_ReturnsFailure_WhenProductNotFound()
    {
        var dto = new GenerateVariantsDTO
        {
            ProductId = Guid.NewGuid(),
            Attributes =
            [
                new VariantAttributeInputDTO { AttributeId = Guid.NewGuid(), Values = ["M"] }
            ]
        };

        _productRepository.Setup(x => x.GetByIdAsync(dto.ProductId)).ReturnsAsync((Product?)null);

        var sut = CreateService();
        var result = await sut.GenerateAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Product not found", result.Message);
        _variantRepository.Verify(x => x.DeleteByProductIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsFailure_WhenNoAttributesProvided()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Tee", BasePrice = 10m };
        var dto = new GenerateVariantsDTO { ProductId = product.Id, Attributes = [] };

        _productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var sut = CreateService();
        var result = await sut.GenerateAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("No attributes provided", result.Message);
        _variantRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAsync_BuildsCartesianVariants_AndPersists()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Shirt", BasePrice = 25m };
        var colorId = Guid.NewGuid();
        var sizeId = Guid.NewGuid();
        var dto = new GenerateVariantsDTO
        {
            ProductId = product.Id,
            Attributes =
            [
                new VariantAttributeInputDTO { AttributeId = colorId, Values = ["Red", "Blue"] },
                new VariantAttributeInputDTO { AttributeId = sizeId, Values = ["S", "M"] }
            ]
        };

        _productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _variantRepository.Setup(x => x.GetByProductIdAsync(product.Id))
                          .ReturnsAsync(new List<ProductVariant>());

        IEnumerable<ProductVariant>? savedVariants = null;
        _variantRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()))
            .Callback<IEnumerable<ProductVariant>>(v => savedVariants = v);

        var sut = CreateService();
        var result = await sut.GenerateAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("Variants generated successfully", result.Message);
        _variantRepository.Verify(x => x.DeleteByProductIdAsync(product.Id), Times.Once);
        _variantRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()), Times.Once);
        _productRepository.Verify(x => x.SaveAsync(), Times.Once);

        var variants = Assert.IsAssignableFrom<IEnumerable<ProductVariant>>(savedVariants).ToList();
        Assert.Equal(4, variants.Count);
        Assert.All(variants, v =>
        {
            Assert.Equal(product.Id, v.ProductId);
            Assert.Equal(25m, v.Price);
            Assert.StartsWith("SHI-", v.Sku);
            Assert.Equal(2, v.AttributeValues.Count);
        });
    }

    [Fact]
    public async Task GetByProductIdAsync_MapsVariantsToDictionaryAttributes()
    {
        var productId = Guid.NewGuid();
        var variants = new List<ProductVariant>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Price = 11,
                StockQuantity = 5,
                IsActive = true,
                AttributeValues =
                [
                    new VariantAttributeValue { VariantAttribute = new VariantAttribute { Name = "Color" }, Value = "Green" },
                    new VariantAttributeValue { VariantAttribute = new VariantAttribute { Name = "Size" }, Value = "L" }
                ]
            }
        };

        _variantRepository.Setup(x => x.GetByProductIdAsync(productId)).ReturnsAsync(variants);

        var sut = CreateService();
        var result = await sut.GetByProductIdAsync(productId);

        var dto = Assert.Single(result);
        Assert.Equal("Green", dto.Attributes["Color"]);
        Assert.Equal("L", dto.Attributes["Size"]);
    }

    // ── Stock inheritance ────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateAsync_SplitsStock_WhenNewDimensionAdded()
    {
        // Existing: S=10, M=6  (size only)
        // New:      Size×Color → S×Red, S×Blue, M×Red, M×Blue
        // Expected: S×Red=5, S×Blue=5, M×Red=3, M×Blue=3

        var sizeId  = Guid.NewGuid();
        var colorId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Tee", BasePrice = 20m };

        var existingS = ExistingVariant(product.Id, sizeId, "S", stock: 10, price: 20m);
        var existingM = ExistingVariant(product.Id, sizeId, "M", stock: 6,  price: 20m);

        _productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _variantRepository.Setup(x => x.GetByProductIdAsync(product.Id))
                          .ReturnsAsync(new List<ProductVariant> { existingS, existingM });

        IEnumerable<ProductVariant>? saved = null;
        _variantRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()))
                          .Callback<IEnumerable<ProductVariant>>(v => saved = v);

        var dto = new GenerateVariantsDTO
        {
            ProductId = product.Id,
            Attributes =
            [
                new VariantAttributeInputDTO { AttributeId = sizeId,  Values = ["S", "M"] },
                new VariantAttributeInputDTO { AttributeId = colorId, Values = ["Red", "Blue"] }
            ]
        };

        await CreateService().GenerateAsync(dto);

        var variants = saved!.ToList();
        Assert.Equal(4, variants.Count);
        Assert.Equal(5, StockOf(variants, sizeId, "S", colorId, "Red"));
        Assert.Equal(5, StockOf(variants, sizeId, "S", colorId, "Blue"));
        Assert.Equal(3, StockOf(variants, sizeId, "M", colorId, "Red"));
        Assert.Equal(3, StockOf(variants, sizeId, "M", colorId, "Blue"));
    }

    [Fact]
    public async Task GenerateAsync_SumsStock_WhenDimensionRemoved()
    {
        // Existing: S×Red=5, S×Blue=5  (size+color)
        // New:      Size only → S
        // Expected: S=10

        var sizeId  = Guid.NewGuid();
        var colorId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Tee", BasePrice = 20m };

        var existingSRed  = ExistingVariant(product.Id, sizeId, "S", stock: 5, price: 20m, colorId, "Red");
        var existingSBlue = ExistingVariant(product.Id, sizeId, "S", stock: 5, price: 20m, colorId, "Blue");

        _productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _variantRepository.Setup(x => x.GetByProductIdAsync(product.Id))
                          .ReturnsAsync(new List<ProductVariant> { existingSRed, existingSBlue });

        IEnumerable<ProductVariant>? saved = null;
        _variantRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()))
                          .Callback<IEnumerable<ProductVariant>>(v => saved = v);

        var dto = new GenerateVariantsDTO
        {
            ProductId = product.Id,
            Attributes = [new VariantAttributeInputDTO { AttributeId = sizeId, Values = ["S"] }]
        };

        await CreateService().GenerateAsync(dto);

        var variants = saved!.ToList();
        var sVariant = Assert.Single(variants);
        Assert.Equal(10, sVariant.StockQuantity); // 5+5
    }

    [Fact]
    public async Task GenerateAsync_CopiesStock_WhenSameDimensions()
    {
        // Existing: S×Red=7, S×Blue=3  (regenerate same structure)
        // Expected: S×Red=7, S×Blue=3  (unchanged)

        var sizeId  = Guid.NewGuid();
        var colorId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Tee", BasePrice = 20m };

        var existingSRed  = ExistingVariant(product.Id, sizeId, "S", stock: 7, price: 25m, colorId, "Red");
        var existingSBlue = ExistingVariant(product.Id, sizeId, "S", stock: 3, price: 25m, colorId, "Blue");

        _productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _variantRepository.Setup(x => x.GetByProductIdAsync(product.Id))
                          .ReturnsAsync(new List<ProductVariant> { existingSRed, existingSBlue });

        IEnumerable<ProductVariant>? saved = null;
        _variantRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()))
                          .Callback<IEnumerable<ProductVariant>>(v => saved = v);

        var dto = new GenerateVariantsDTO
        {
            ProductId = product.Id,
            Attributes =
            [
                new VariantAttributeInputDTO { AttributeId = sizeId,  Values = ["S"] },
                new VariantAttributeInputDTO { AttributeId = colorId, Values = ["Red", "Blue"] }
            ]
        };

        await CreateService().GenerateAsync(dto);

        var variants = saved!.ToList();
        Assert.Equal(7, StockOf(variants, sizeId, "S", colorId, "Red"));
        Assert.Equal(3, StockOf(variants, sizeId, "S", colorId, "Blue"));
        // Price is also inherited
        Assert.All(variants, v => Assert.Equal(25m, v.Price));
    }

    [Fact]
    public async Task GenerateAsync_ZeroStock_WhenNoMatchingExistingVariant()
    {
        // Existing: S×Red=10
        // New:      S×Green  (brand-new color value)
        // Expected: S×Green=0

        var sizeId  = Guid.NewGuid();
        var colorId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Tee", BasePrice = 20m };

        var existingSRed = ExistingVariant(product.Id, sizeId, "S", stock: 10, price: 20m, colorId, "Red");

        _productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _variantRepository.Setup(x => x.GetByProductIdAsync(product.Id))
                          .ReturnsAsync(new List<ProductVariant> { existingSRed });

        IEnumerable<ProductVariant>? saved = null;
        _variantRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductVariant>>()))
                          .Callback<IEnumerable<ProductVariant>>(v => saved = v);

        var dto = new GenerateVariantsDTO
        {
            ProductId = product.Id,
            Attributes =
            [
                new VariantAttributeInputDTO { AttributeId = sizeId,  Values = ["S"] },
                new VariantAttributeInputDTO { AttributeId = colorId, Values = ["Green"] }
            ]
        };

        await CreateService().GenerateAsync(dto);

        var green = saved!.Single();
        Assert.Equal(0, green.StockQuantity);
    }

    // ── Test helpers ─────────────────────────────────────────────────────────

    private static ProductVariant ExistingVariant(
        Guid productId,
        Guid attrId1, string val1,
        int stock, decimal price,
        Guid attrId2 = default, string val2 = "")
    {
        var v = new ProductVariant
        {
            Id            = Guid.NewGuid(),
            ProductId     = productId,
            StockQuantity = stock,
            Price         = price,
            IsActive      = true
        };
        v.AttributeValues = new List<VariantAttributeValue>
        {
            new() { ProductVariant = v, VariantAttributeId = attrId1, Value = val1 }
        };
        if (attrId2 != default && !string.IsNullOrEmpty(val2))
            v.AttributeValues.Add(new() { ProductVariant = v, VariantAttributeId = attrId2, Value = val2 });
        return v;
    }

    private static int StockOf(
        List<ProductVariant> variants,
        Guid attrId1, string val1,
        Guid attrId2, string val2) =>
        variants.First(v =>
            v.AttributeValues.Any(av => av.VariantAttributeId == attrId1 && av.Value == val1) &&
            v.AttributeValues.Any(av => av.VariantAttributeId == attrId2 && av.Value == val2))
        .StockQuantity;

    [Fact]
    public async Task UpdateVariantAsync_ReturnsFailure_WhenVariantMissing()
    {
        var dto = new UpdateVariantDTO { VariantId = Guid.NewGuid(), Price = 44, Stock = 9, IsActive = false };
        _variantRepository.Setup(x => x.GetByIdAsync(dto.VariantId)).ReturnsAsync((ProductVariant?)null);

        var sut = CreateService();
        var result = await sut.UpdateVariantAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Variant not found", result.Message);
        _productRepository.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateVariantAsync_UpdatesFields_AndSaves()
    {
        var existing = new ProductVariant { Id = Guid.NewGuid(), Price = 10, StockQuantity = 1, IsActive = true };
        var dto = new UpdateVariantDTO { VariantId = existing.Id, Price = 99, Stock = 12, IsActive = false };

        _variantRepository.Setup(x => x.GetByIdAsync(existing.Id)).ReturnsAsync(existing);

        var sut = CreateService();
        var result = await sut.UpdateVariantAsync(dto);

        Assert.True(result.Success);
        Assert.Equal(99, existing.Price);
        Assert.Equal(12, existing.StockQuantity);
        Assert.False(existing.IsActive);
        _productRepository.Verify(x => x.SaveAsync(), Times.Once);
    }
}
