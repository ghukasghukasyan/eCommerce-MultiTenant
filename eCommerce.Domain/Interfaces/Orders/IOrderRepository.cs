using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.QueryFilters;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Domain.Interfaces.Orders
{
    public interface IOrderRepository
    {
        Task<int> SaveAsync(Order order);
        Task<int> UpdateStatus(Guid id, OrderStatus status);
        Task<int> UpdateCheckoutAsync(Guid id);
        Task<int> CountByFilterAsync(OrderQueryFilter filter);
        Task<Order> GetByIdAsync(Guid orderId);
        Task<IEnumerable<Order>> GetAsync(OrderQueryFilter filter, int skip, int take);

        // Dashboard aggregates
        Task<int> CountAllAsync();
        Task<decimal> SumRevenueAsync();
        Task<int> CountTodayAsync(DateTime todayUtc);
        Task<decimal> SumTodayRevenueAsync(DateTime todayUtc);
        Task<int> CountByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetRecentAsync(int count);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
