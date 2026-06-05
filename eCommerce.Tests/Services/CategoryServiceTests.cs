using AutoMapper;
using eCommerce.Application.DTOs.Categories;
using eCommerce.Application.Services.Implementations.Categories;
using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces;
using eCommerce.Domain.Interfaces.Categories;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace eCommerce.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IGeneric<Category>> _genericRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    private CategoryService CreateService() => new(_genericRepo.Object, _mapper.Object, _categoryRepo.Object, _cache);

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenNotFound()
    {
        var dto = new UpdateCategoryDTO { Id = Guid.NewGuid(), Name = "A" };
        _genericRepo.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync((Category?)null);

        var result = await CreateService().UpdateAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Category not found", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenSelfParent()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateCategoryDTO { Id = id, Name = "A", ParentCategoryId = id };
        _genericRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new Category { Id = id, Name = "Old" });

        var result = await CreateService().UpdateAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Category cannot be its own parent", result.Message);
        _genericRepo.Verify(x => x.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity_WhenValid()
    {
        var entity = new Category { Id = Guid.NewGuid(), Name = "Old" };
        var dto = new UpdateCategoryDTO
        {
            Id = entity.Id,
            Name = "New",
            ParentCategoryId = Guid.NewGuid(),
            DisplayOrder = 2,
            IsActive = false
        };

        _genericRepo.Setup(x => x.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        _genericRepo.Setup(x => x.UpdateAsync(entity)).ReturnsAsync(1);

        var result = await CreateService().UpdateAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("New", entity.Name);
        Assert.Equal(dto.ParentCategoryId, entity.ParentCategoryId);
        Assert.Equal(2, entity.DisplayOrder);
        Assert.False(entity.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoData()
    {
        _categoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await CreateService().GetAllAsync();

        Assert.Empty(result);
        _mapper.Verify(x => x.Map<IEnumerable<GetCategoryDTO>>(It.IsAny<IEnumerable<Category>>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_MapsData_WhenAvailable()
    {
        var entities = new List<Category> { new() { Id = Guid.NewGuid(), Name = "Shoes" } };
        var dtos = new List<GetCategoryDTO> { new() { Id = entities[0].Id, Name = "Shoes" } };

        _categoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(entities);
        _mapper.Setup(x => x.Map<IEnumerable<GetCategoryDTO>>(entities)).Returns(dtos);

        var result = await CreateService().GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsEmpty_WhenNoProducts()
    {
        _categoryRepo.Setup(x => x.GetProductsByCategoryIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Product>());

        var result = await CreateService().GetProductsByCategory(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDefaultDto_WhenMissing()
    {
        _categoryRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var result = await CreateService().GetByIdAsync(Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
    }
}
