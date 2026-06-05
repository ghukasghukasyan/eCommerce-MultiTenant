using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Coupons
{
    public class UpdateCouponDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int CommissionRate { get; set; }
        public int? MaxUsages { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
