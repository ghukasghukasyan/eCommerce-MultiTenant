using eCommerce.Domain.Entities.Coupons;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Coupons
{
    public class CouponRepository(ECommerceContext context) : ICouponRepository
    {
        public async Task<List<Coupon>> GetAllAsync()
            => await context.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync();

        public async Task<List<Coupon>> GetByInfluencerAsync(Guid influencerId)
            => await context.Coupons
                .Where(c => c.InfluencerId == influencerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<Coupon?> GetByIdAsync(Guid id)
            => await context.Coupons.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Coupon?> GetByCodeAsync(string code)
            => await context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

        public async Task AddAsync(Coupon coupon)
            => await context.Coupons.AddAsync(coupon);

        public Task UpdateAsync(Coupon coupon)
        {
            context.Coupons.Update(coupon);
            return Task.CompletedTask;
        }

        public async Task AddCouponOrderAsync(CouponOrder couponOrder)
            => await context.CouponOrders.AddAsync(couponOrder);

        public async Task<CouponOrder?> GetCouponOrderByOrderIdAsync(Guid orderId)
            => await context.CouponOrders.FirstOrDefaultAsync(co => co.OrderId == orderId);

        public async Task<List<CouponOrder>> GetCouponOrdersByInfluencerAsync(Guid influencerId)
        {
            var couponIds = await context.Coupons
                .Where(c => c.InfluencerId == influencerId)
                .Select(c => c.Id)
                .ToListAsync();

            return await context.CouponOrders
                .Where(co => couponIds.Contains(co.CouponId))
                .ToListAsync();
        }

        public async Task SaveAsync()
            => await context.SaveChangesAsync();
    }
}
