namespace ClientLibrary.Models.Influencers
{
    public class CouponPayoutDTO
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
        public int CommissionRate { get; set; }
        public int UsedCount { get; set; }
        public decimal TotalEarned { get; set; }
    }
}
