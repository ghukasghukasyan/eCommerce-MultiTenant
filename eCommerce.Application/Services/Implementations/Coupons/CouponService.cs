using eCommerce.Application.DTOs.Coupons;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Coupons;
using eCommerce.Domain.Entities.Coupons;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Influencers;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.Services.Implementations.Coupons
{
    public class CouponService(ICouponRepository couponRepository, IInfluencerRepository influencerRepository) : ICouponService
    {
        public async Task<List<GetCouponDTO>> GetAllAsync()
        {
            var coupons = await couponRepository.GetAllAsync();
            return await MapWithInfluencerNamesAsync(coupons);
        }

        public async Task<List<GetCouponDTO>> GetByInfluencerAsync(Guid influencerId)
        {
            var coupons = await couponRepository.GetByInfluencerAsync(influencerId);
            return await MapWithInfluencerNamesAsync(coupons);
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateCouponDTO dto)
        {
            var existing = await couponRepository.GetByCodeAsync(dto.Code);
            if (existing != null)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Coupon code already exists.");

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = dto.Code.ToUpper().Trim(),
                InfluencerId = dto.InfluencerId,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                CommissionRate = dto.CommissionRate,
                MaxUsages = dto.MaxUsages,
                ExpiryDate = dto.ExpiryDate?.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow,
                UsedCount = 0,
                Status = ActivityStatus.Active
            };

            await couponRepository.AddAsync(coupon);
            await couponRepository.SaveAsync();

            return new ServiceResponse<Guid>(true, coupon.Id, "Coupon created successfully.");
        }

        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateCouponDTO dto)
        {
            var coupon = await couponRepository.GetByIdAsync(dto.Id);
            if (coupon == null)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Coupon not found.");

            var existing = await couponRepository.GetByCodeAsync(dto.Code);
            if (existing != null && existing.Id != dto.Id)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Coupon code already exists.");

            coupon.Code = dto.Code.ToUpper().Trim();
            coupon.DiscountType = dto.DiscountType;
            coupon.DiscountValue = dto.DiscountValue;
            coupon.CommissionRate = dto.CommissionRate;
            coupon.MaxUsages = dto.MaxUsages;
            coupon.ExpiryDate = dto.ExpiryDate?.ToUniversalTime();

            await couponRepository.UpdateAsync(coupon);
            await couponRepository.SaveAsync();

            return new ServiceResponse<Guid>(true, coupon.Id, "Coupon updated successfully.");
        }

        public async Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateCouponStatusDTO dto)
        {
            var coupon = await couponRepository.GetByIdAsync(dto.Id);
            if (coupon == null)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Coupon not found.");

            coupon.Status = dto.Status;
            await couponRepository.UpdateAsync(coupon);
            await couponRepository.SaveAsync();

            return new ServiceResponse<Guid>(true, coupon.Id, "Status updated successfully.");
        }

        public async Task<CouponValidationResultDTO> ValidateAsync(ValidateCouponDTO dto)
        {
            var coupon = await couponRepository.GetByCodeAsync(dto.Code);

            if (coupon == null)
                return Invalid("Coupon not found.");

            if (coupon.Status != ActivityStatus.Active)
                return Invalid("This coupon is no longer active.");

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.UtcNow)
                return Invalid("This coupon has expired.");

            if (coupon.MaxUsages.HasValue && coupon.UsedCount >= coupon.MaxUsages.Value)
                return Invalid("This coupon has reached its usage limit.");

            var discountAmount = CalculateDiscount(coupon, dto.OrderTotal);

            return new CouponValidationResultDTO
            {
                IsValid = true,
                Message = "Coupon applied successfully.",
                CouponId = coupon.Id,
                DiscountAmount = discountAmount,
                DiscountedTotal = Math.Max(0, dto.OrderTotal - discountAmount)
            };
        }

        public static decimal CalculateDiscount(Coupon coupon, decimal orderTotal)
        {
            if (coupon.DiscountType == DiscountType.Percentage)
                return Math.Round(orderTotal * coupon.DiscountValue / 100m, 2);

            return Math.Min(coupon.DiscountValue, orderTotal);
        }

        private async Task<List<GetCouponDTO>> MapWithInfluencerNamesAsync(List<Coupon> coupons)
        {
            var influencerIds = coupons.Select(c => c.InfluencerId).Distinct().ToList();
            var influencers = await influencerRepository.GetByIdsAsync(influencerIds);
            var influencerMap = influencers.ToDictionary(i => i.Id, i => i.FullName);

            return coupons.Select(c => new GetCouponDTO
            {
                Id = c.Id,
                Code = c.Code,
                InfluencerId = c.InfluencerId,
                InfluencerName = influencerMap.TryGetValue(c.InfluencerId, out var name) ? name : "Unknown",
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MaxUsages = c.MaxUsages,
                CommissionRate = c.CommissionRate,
                ExpiryDate = c.ExpiryDate,
                CreatedAt = c.CreatedAt,
                UsedCount = c.UsedCount,
                Status = c.Status
            }).ToList();
        }

        private static CouponValidationResultDTO Invalid(string message)
            => new() { IsValid = false, Message = message };
    }
}
