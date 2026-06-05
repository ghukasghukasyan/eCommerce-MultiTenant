using eCommerce.Application.DTOs.Dashboard;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Influencers;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.Interfaces.Products;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.Services.Implementations
{
    public class DashboardService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInfluencerRepository influencerRepository,
        IUserManagement userManagement
    ) : IDashboardService
    {
        public async Task<DashboardStatsDTO> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-6);

            var todayOrderCount   = await orderRepository.CountTodayAsync(today);
            var todayRevenue      = await orderRepository.SumTodayRevenueAsync(today);
            var totalOrders       = await orderRepository.CountAllAsync();
            var totalRevenue      = await orderRepository.SumRevenueAsync();
            var pendingOrders     = await orderRepository.CountByStatusAsync(OrderStatus.Pending);
            var totalProducts     = await productRepository.CountAsync();
            var approvedInfluencers = await influencerRepository.CountByStatusAsync(InfluencerStatus.Approved);

            var totalUsers = await userManagement.CountAsync();

            var recentOrders = (await orderRepository.GetRecentAsync(6))
                .Select(o => new DashboardRecentOrderDTO
                {
                    Id           = o.Id,
                    CustomerName = o.CustomerName ?? "—",
                    TotalAmount  = o.TotalAmount,
                    Status       = o.Status,
                    CreatedAt    = o.CreatedAt
                })
                .ToList();

            var rawOrders = await orderRepository.GetByDateRangeAsync(weekAgo, today.AddDays(1));
            var rawWeek = rawOrders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DashboardDailyRevenueDTO
                {
                    Date    = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders  = g.Count()
                })
                .ToList();

            var weeklyRevenue = Enumerable.Range(0, 7)
                .Select(d =>
                {
                    var date  = today.AddDays(-6 + d);
                    var found = rawWeek.FirstOrDefault(w => w.Date == date);
                    return new DashboardDailyRevenueDTO
                    {
                        Date    = date,
                        Revenue = found?.Revenue ?? 0,
                        Orders  = found?.Orders  ?? 0
                    };
                })
                .ToList();

            return new DashboardStatsDTO
            {
                TodayOrders         = todayOrderCount,
                TodayRevenue        = todayRevenue,
                TotalUsers          = totalUsers,
                PendingOrders       = pendingOrders,
                TotalRevenue        = totalRevenue,
                TotalOrders         = totalOrders,
                TotalProducts       = totalProducts,
                ApprovedInfluencers = approvedInfluencers,
                RecentOrders        = recentOrders,
                WeeklyRevenue       = weeklyRevenue
            };
        }
    }
}
