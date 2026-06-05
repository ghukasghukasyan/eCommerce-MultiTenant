namespace eCommerce.Application.DTOs.Influencers
{
    public class CreateInfluencerPayoutDTO
    {
        public Guid InfluencerId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
