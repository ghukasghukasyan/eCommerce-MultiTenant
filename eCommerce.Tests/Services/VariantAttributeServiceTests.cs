using eCommerce.Application.DTOs.Products.VariantAttributes;
using eCommerce.Application.Services.Implementations.Products;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using Moq;

namespace eCommerce.Tests.Services;

public class VariantAttributeServiceTests
{
    private readonly Mock<IVariantAttributeRepository> _repo = new();

    [Fact]
    public async Task CreateAsync_PersistsAttribute_AndReturnsId()
    {
        VariantAttribute? saved = null;
        _repo.Setup(x => x.AddAsync(It.IsAny<VariantAttribute>()))
            .Callback<VariantAttribute>(x => saved = x)
            .Returns(Task.CompletedTask);

        var sut = new VariantAttributeService(_repo.Object);

        var result = await sut.CreateAsync(new CreateVariantAttributeDTO { Name = "Color" });

        Assert.True(result.Success);
        Assert.Equal("Attribute created", result.Message);
        Assert.NotNull(saved);
        Assert.Equal("Color", saved!.Name);
        Assert.Equal(saved.Id, result.Data);
        _repo.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_MapsEntitiesToDtos()
    {
        var entities = new List<VariantAttribute>
        {
            new() { Id = Guid.NewGuid(), Name = "Color" },
            new() { Id = Guid.NewGuid(), Name = "Size" }
        };

        _repo.Setup(x => x.GetAllAsync()).ReturnsAsync(entities);

        var sut = new VariantAttributeService(_repo.Object);
        var result = await sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Color");
        Assert.Contains(result, x => x.Name == "Size");
    }
}
