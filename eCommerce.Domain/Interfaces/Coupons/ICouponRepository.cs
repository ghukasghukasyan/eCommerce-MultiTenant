using eCommerce.Domain.Entities.Coupons;
using eCommerce.Domain.Entities.Orders;

namespace eCommerce.Domain.Interfaces.Coupons
{
    public interface ICouponRepository
    {
        Task<List<Coupon>> GetAllAsync();
        Task<List<Coupon>> GetByInfluencerAsync(Guid influencerId);
        Task<Coupon?> GetByIdAsync(Guid id);
        Task<Coupon?> GetByCodeAsync(string code);
        Task AddAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task AddCouponOrderAsync(CouponOrder couponOrder);
        Task<CouponOrder?> GetCouponOrderByOrderIdAsync(Guid orderId);
        Task<List<CouponOrder>> GetCouponOrdersByInfluencerAsync(Guid influencerId);
        Task SaveAsync();
    }
}
