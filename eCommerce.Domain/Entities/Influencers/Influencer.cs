using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Domain.Entities.Influencers
{
    public class Influencer
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int DefaultCommissionRate { get; set; }
        public string AvatarUrl { get; set; }
        public string InstagramAccountUrl { get; set; }
        public string TikTokAccountUrl { get; set; }
        public InfluencerStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
