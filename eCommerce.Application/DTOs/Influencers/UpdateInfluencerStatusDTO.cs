using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Influencers
{
    public class UpdateInfluencerStatusDTO
    {
        public Guid Id { get; set; }
        public InfluencerStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
