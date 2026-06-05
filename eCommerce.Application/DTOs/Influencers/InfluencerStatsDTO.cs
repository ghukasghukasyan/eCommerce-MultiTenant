namespace eCommerce.Application.DTOs.Influencers
{
    public class InfluencerStatsDTO
    {
        public decimal TotalEarned { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal PendingBalance => TotalEarned - TotalPaid;
        public int TotalOrders { get; set; }
        public int ActiveCoupons { get; set; }
        public List<CouponPayoutDTO> Payouts { get; set; } = new();
        public List<GetInfluencerPayoutDTO> PayoutHistory { get; set; } = new();
    }
}
