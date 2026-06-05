using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.QueryFilters;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Users
{
    public class UserOrderRepository(ECommerceContext context) : IUserOrderRepository
    {
        public async Task<IEnumerable<Order>> GetAsync(OrderQueryFilter filter, string userId, int skip, int take)
        {
            var query = ApplyFilter(context.Orders.AsNoTracking().Include(x => x.Items), filter, userId);

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(Guid orderId, string userId)
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
               .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<int> CountByFilterAsync(OrderQueryFilter filter, string userId)
        {
            var query = ApplyFilter(context.Orders.AsQueryable(), filter, userId);
            return await query.CountAsync();
        }
       
        private static IQueryable<Order> ApplyFilter(
      IQueryable<Order> query,
      OrderQueryFilter filter, string userId)
        {
            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status);

            if (filter.From.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.To.Value);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.UserId == userId);
            }

            return query;
        }
    }
}
