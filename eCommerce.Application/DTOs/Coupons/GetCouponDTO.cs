using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Coupons
{
    public class GetCouponDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid InfluencerId { get; set; }
        public string InfluencerName { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int CommissionRate { get; set; }
        public int? MaxUsages { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UsedCount { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
