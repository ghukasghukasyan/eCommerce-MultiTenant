using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Coupons
{
    public class CreateCouponDTO
    {
        public string Code { get; set; }
        public Guid InfluencerId { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int CommissionRate { get; set; }
        public int? MaxUsages { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
