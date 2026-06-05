namespace eCommerce.Application.DTOs.Influencers
{
    public class GetInfluencerPayoutDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
