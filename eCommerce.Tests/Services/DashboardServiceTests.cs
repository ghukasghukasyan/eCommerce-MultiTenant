using eCommerce.Application.Services.Implementations;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Influencers;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.Interfaces.Products;
using Moq;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IInfluencerRepository> _influencerRepo = new();
    private readonly Mock<IUserManagement> _userManagement = new();

    private DashboardService CreateService() => new(
        _orderRepo.Object,
        _productRepo.Object,
        _influencerRepo.Object,
        _userManagement.Object);

    private void SetupDefaults(
        int totalOrders = 0, decimal totalRevenue = 0,
        int todayOrders = 0, decimal todayRevenue = 0,
        int pending = 0, int products = 0, int influencers = 0,
        int users = 0,
        IEnumerable<Order>? recent = null,
        IEnumerable<Order>? weeklyOrders = null)
    {
        _orderRepo.Setup(x => x.CountAllAsync()).ReturnsAsync(totalOrders);
        _orderRepo.Setup(x => x.SumRevenueAsync()).ReturnsAsync(totalRevenue);
        _orderRepo.Setup(x => x.CountTodayAsync(It.IsAny<DateTime>())).ReturnsAsync(todayOrders);
        _orderRepo.Setup(x => x.SumTodayRevenueAsync(It.IsAny<DateTime>())).ReturnsAsync(todayRevenue);
        _orderRepo.Setup(x => x.CountByStatusAsync(OrderStatus.Pending)).ReturnsAsync(pending);
        _orderRepo.Setup(x => x.GetRecentAsync(6)).ReturnsAsync(recent ?? []);
        _orderRepo.Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(weeklyOrders ?? []);

        _productRepo.Setup(x => x.CountAsync()).ReturnsAsync(products);
        _influencerRepo.Setup(x => x.CountByStatusAsync(InfluencerStatus.Approved)).ReturnsAsync(influencers);
        _userManagement.Setup(x => x.CountAsync()).ReturnsAsync(users);
    }

    // ── Scalar stats ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatsAsync_ReturnsAllScalarCounts()
    {
        SetupDefaults(
            totalOrders: 42,
            totalRevenue: 980_000,
            todayOrders: 5,
            todayRevenue: 75_000,
            pending: 3,
            products: 18,
            influencers: 4,
            users: 3);

        var result = await CreateService().GetStatsAsync();

        Assert.Equal(42, result.TotalOrders);
        Assert.Equal(980_000m, result.TotalRevenue);
        Assert.Equal(5, result.TodayOrders);
        Assert.Equal(75_000m, result.TodayRevenue);
        Assert.Equal(3, result.PendingOrders);
        Assert.Equal(18, result.TotalProducts);
        Assert.Equal(4, result.ApprovedInfluencers);
        Assert.Equal(3, result.TotalUsers);
    }

    // ── Recent orders ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatsAsync_MapsRecentOrders_WithCustomerNameFallback()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        SetupDefaults(recent:
        [
            new Order { Id = id1, CustomerName = "Alice", TotalAmount = 5000, Status = OrderStatus.Paid, CreatedAt = DateTime.UtcNow },
            new Order { Id = id2, CustomerName = null,    TotalAmount = 2000, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow }
        ]);

        var result = await CreateService().GetStatsAsync();

        Assert.Equal(2, result.RecentOrders.Count);

        var alice = result.RecentOrders.First(o => o.Id == id1);
        Assert.Equal("Alice", alice.CustomerName);
        Assert.Equal(5000m, alice.TotalAmount);
        Assert.Equal(OrderStatus.Paid, alice.Status);

        var anon = result.RecentOrders.First(o => o.Id == id2);
        Assert.Equal("—", anon.CustomerName);
    }

    [Fact]
    public async Task GetStatsAsync_RecentOrders_IsEmptyList_WhenNoOrders()
    {
        SetupDefaults();

        var result = await CreateService().GetStatsAsync();

        Assert.Empty(result.RecentOrders);
    }

    // ── Weekly revenue ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatsAsync_WeeklyRevenue_AlwaysHasSevenEntries()
    {
        SetupDefaults();

        var result = await CreateService().GetStatsAsync();

        Assert.Equal(7, result.WeeklyRevenue.Count);
    }

    [Fact]
    public async Task GetStatsAsync_WeeklyRevenue_EntriesAreConsecutiveDays()
    {
        SetupDefaults();

        var result = await CreateService().GetStatsAsync();

        var today = DateTime.UtcNow.Date;
        for (int i = 0; i < 7; i++)
            Assert.Equal(today.AddDays(-6 + i), result.WeeklyRevenue[i].Date);
    }

    [Fact]
    public async Task GetStatsAsync_WeeklyRevenue_GapFillsEmptyDays_WithZero()
    {
        var today = DateTime.UtcNow.Date;

        // Only two days have orders
        SetupDefaults(weeklyOrders:
        [
            new Order { CreatedAt = today.AddDays(-5), TotalAmount = 1000 },
            new Order { CreatedAt = today.AddDays(-5), TotalAmount = 500 },
            new Order { CreatedAt = today,             TotalAmount = 2000 }
        ]);

        var result = await CreateService().GetStatsAsync();

        Assert.Equal(7, result.WeeklyRevenue.Count);

        var dayMinus5 = result.WeeklyRevenue.Single(w => w.Date == today.AddDays(-5));
        Assert.Equal(1500m, dayMinus5.Revenue);
        Assert.Equal(2, dayMinus5.Orders);

        var todayEntry = result.WeeklyRevenue.Single(w => w.Date == today);
        Assert.Equal(2000m, todayEntry.Revenue);
        Assert.Equal(1, todayEntry.Orders);

        // All other days should be zero
        var emptyDays = result.WeeklyRevenue.Where(w => w.Date != today.AddDays(-5) && w.Date != today);
        Assert.All(emptyDays, w =>
        {
            Assert.Equal(0m, w.Revenue);
            Assert.Equal(0, w.Orders);
        });
    }

    [Fact]
    public async Task GetStatsAsync_WeeklyRevenue_AggregatesMultipleOrdersOnSameDay()
    {
        var today = DateTime.UtcNow.Date;

        SetupDefaults(weeklyOrders:
        [
            new Order { CreatedAt = today.AddDays(-1), TotalAmount = 100 },
            new Order { CreatedAt = today.AddDays(-1), TotalAmount = 200 },
            new Order { CreatedAt = today.AddDays(-1), TotalAmount = 300 }
        ]);

        var result = await CreateService().GetStatsAsync();

        var yesterday = result.WeeklyRevenue.Single(w => w.Date == today.AddDays(-1));
        Assert.Equal(600m, yesterday.Revenue);
        Assert.Equal(3, yesterday.Orders);
    }

    // ── Repository calls ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatsAsync_CallsGetRecentAsync_WithCountOf6()
    {
        SetupDefaults();

        await CreateService().GetStatsAsync();

        _orderRepo.Verify(x => x.GetRecentAsync(6), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_CallsGetByDateRange_WithSevenDayWindow()
    {
        SetupDefaults();

        await CreateService().GetStatsAsync();

        var expectedFrom = DateTime.UtcNow.Date.AddDays(-6);
        var expectedTo   = DateTime.UtcNow.Date.AddDays(1);

        _orderRepo.Verify(x => x.GetByDateRangeAsync(
            It.Is<DateTime>(d => d.Date == expectedFrom),
            It.Is<DateTime>(d => d.Date == expectedTo)),
            Times.Once);
    }
}
