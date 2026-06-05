using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.QueryFilters;

namespace eCommerce.Domain.Interfaces.Orders
{
    public interface IUserOrderRepository
    {
        Task<Order> GetByIdAsync(Guid orderId, string userId);
        Task<IEnumerable<Order>> GetAsync(OrderQueryFilter filter, string userId, int skip, int take);
        Task<int> CountByFilterAsync(OrderQueryFilter filter, string userId);
    }
}
