namespace eCommerce.Domain.Entities.Influencers
{
    public class InfluencerPayout
    {
        public Guid Id { get; set; }
        public Guid InfluencerId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
