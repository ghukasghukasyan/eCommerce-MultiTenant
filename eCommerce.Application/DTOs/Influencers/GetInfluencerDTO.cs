using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Influencers
{
    public class GetInfluencerDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? InstagramAccountUrl { get; set; }
        public string? TikTokAccountUrl { get; set; }
        public decimal DefaultCommissionRate { get; set; }
        public InfluencerStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
