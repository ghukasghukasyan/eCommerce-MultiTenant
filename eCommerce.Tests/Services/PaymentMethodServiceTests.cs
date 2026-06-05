using AutoMapper;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.Services.Implementations.Orders;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Orders;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace eCommerce.Tests.Services;

public class PaymentMethodServiceTests
{
    private readonly Mock<IPaymentMethodRepository> _repo   = new();
    private readonly Mock<IMapper>                  _mapper = new();
    private readonly IMemoryCache                   _cache  = new MemoryCache(new MemoryCacheOptions());

    private PaymentMethodService CreateService() => new(_repo.Object, _mapper.Object, _cache);

    [Fact]
    public async Task GetMethodsAsync_ReturnsEmpty_WhenNoMethodsExist()
    {
        _repo.Setup(r => r.GetPaymentMethodsAsync()).ReturnsAsync(new List<PaymentMethod>());

        var result = await CreateService().GetMethodsAsync();

        Assert.Empty(result);
        _mapper.Verify(m => m.Map<IEnumerable<GetPaymentMethodDTO>>(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task GetMethodsAsync_ReturnsMappedDtos_WhenMethodsExist()
    {
        var methods = new List<PaymentMethod>
        {
            new() { Id = Guid.NewGuid(), Name = "Cash" },
            new() { Id = Guid.NewGuid(), Name = "Card" }
        };
        var dtos = methods.Select(m => new GetPaymentMethodDTO { Id = m.Id, Name = m.Name }).ToList();

        _repo.Setup(r => r.GetPaymentMethodsAsync()).ReturnsAsync(methods);
        _mapper.Setup(m => m.Map<IEnumerable<GetPaymentMethodDTO>>(methods)).Returns(dtos);

        var result = (await CreateService().GetMethodsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Name == "Cash");
        Assert.Contains(result, d => d.Name == "Card");
    }
}
