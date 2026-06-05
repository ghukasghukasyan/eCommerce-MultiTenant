using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.Services.Implementations.Orders;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.QueryFilters;
using Moq;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Tests.Services;

public class UserOrderServiceTests
{
    private readonly Mock<IUserOrderRepository> _repo = new();

    [Fact]
    public async Task GetByIdAsync_UsesSafeDefaults_WhenShippingDetailMissing()
    {
        var service = new UserOrderService(_repo.Object);
        var orderId = Guid.NewGuid();
        _repo.Setup(x => x.GetByIdAsync(orderId, "user-1")).ReturnsAsync(new Order
        {
            Id = orderId,
            UserId = "user-1",
            CustomerName = "Jane",
            CustomerEmail = "jane@example.com",
            ShippingDetail = null!,
            Items = []
        });

        var result = await service.GetByIdAsync(orderId, "user-1");

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result!.ShippingDetail.FullName);
        Assert.Equal(string.Empty, result.ShippingDetail.PhoneNumber);
        Assert.Equal(string.Empty, result.ShippingDetail.City);
        Assert.Equal(string.Empty, result.ShippingDetail.Address);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyPaged_WhenNoOrders()
    {
        _repo.Setup(x => x.CountByFilterAsync(It.IsAny<OrderQueryFilter>(), "u1")).ReturnsAsync(0);

        var service = new UserOrderService(_repo.Object);
        var result  = await service.GetAllAsync(new OrderFilterDTO(), "u1");

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
        _repo.Verify(x => x.GetAsync(It.IsAny<OrderQueryFilter>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedOrders_WhenDataExists()
    {
        var orderId = Guid.NewGuid();
        var order   = new Order
        {
            Id           = orderId,
            CustomerName  = "Jane",
            CustomerEmail = "jane@x.com",
            TotalAmount   = 12_000m,
            Status        = OrderStatus.Paid,
            CreatedAt     = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Items         = []
        };

        _repo.Setup(x => x.CountByFilterAsync(It.IsAny<OrderQueryFilter>(), "u1")).ReturnsAsync(1);
        _repo.Setup(x => x.GetAsync(It.IsAny<OrderQueryFilter>(), "u1", 0, 20)).ReturnsAsync([order]);

        var service = new UserOrderService(_repo.Object);
        var result  = await service.GetAllAsync(new OrderFilterDTO(), "u1");

        Assert.Equal(1, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal(orderId, dto.Id);
        Assert.Equal("Jane", dto.CustomerName);
        Assert.Equal(12_000m, dto.TotalAmount);
        Assert.Equal(OrderStatus.Paid, dto.Status);
    }

    [Fact]
    public async Task GetAllAsync_AppliesPaging_SkipAndTake()
    {
        _repo.Setup(x => x.CountByFilterAsync(It.IsAny<OrderQueryFilter>(), "u1")).ReturnsAsync(50);
        _repo.Setup(x => x.GetAsync(It.IsAny<OrderQueryFilter>(), "u1", 20, 20)).ReturnsAsync([]);

        var service = new UserOrderService(_repo.Object);
        await service.GetAllAsync(new OrderFilterDTO { Page = 2, PageSize = 20 }, "u1");

        _repo.Verify(x => x.GetAsync(It.IsAny<OrderQueryFilter>(), "u1", 20, 20), Times.Once);
    }
}
