using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Enums;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.QueryFilters;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Orders
{
    public class OrderRepository(ECommerceContext context) : IOrderRepository
    {

        public async Task<int> SaveAsync(Order order)
        {
            context.Orders.Add(order);
            return await context.SaveChangesAsync();
        }
        public async Task<int> UpdateStatus(Guid id, Statuses.OrderStatus status)
        {
            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return 0;

            order.Status = status;

            return await context.SaveChangesAsync();
        }

        public async Task<int> UpdateCheckoutAsync(Guid id)
        {
            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return 0;

            order.Status = Statuses.OrderStatus.Paid;
            order.PaymentStatus = Statuses.PaymentStatus.Paid;

            return await context.SaveChangesAsync();
        }
        public async Task<int> CountByFilterAsync(OrderQueryFilter filter)
        {
            var query = ApplyFilter(context.Orders.AsQueryable(), filter);
            return await query.CountAsync();
        }
        public async Task<Order> GetByIdAsync(Guid orderId)
        {
            return await context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.AttributeValues)
                .ThenInclude(av => av.VariantAttribute)
                .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
        public async Task<IEnumerable<Order>> GetAsync(OrderQueryFilter filter, int skip, int take)
        {
            var query = ApplyFilter(context.Orders.AsNoTracking().Include(x => x.Items), filter);

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        public async Task<int> CountAllAsync()
            => await context.Orders.CountAsync();

        public async Task<decimal> SumRevenueAsync()
            => await context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        public async Task<int> CountTodayAsync(DateTime todayUtc)
            => await context.Orders.CountAsync(o => o.CreatedAt >= todayUtc && o.CreatedAt < todayUtc.AddDays(1));

        public async Task<decimal> SumTodayRevenueAsync(DateTime todayUtc)
            => await context.Orders.Where(o => o.CreatedAt >= todayUtc && o.CreatedAt < todayUtc.AddDays(1))
                   .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        public async Task<int> CountByStatusAsync(Statuses.OrderStatus status)
            => await context.Orders.CountAsync(o => o.Status == status);

        public async Task<IEnumerable<Order>> GetRecentAsync(int count)
            => await context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime from, DateTime to)
            => await context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= from && o.CreatedAt < to)
                .ToListAsync();

        private static IQueryable<Order> ApplyFilter(
       IQueryable<Order> query,
       OrderQueryFilter filter)
        {
            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status);

            if (filter.From.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.To.Value);

            return query;
        }
    }
}
