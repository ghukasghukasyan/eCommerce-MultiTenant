using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Influencers
{
    public class UpdateInfluencerStatusDTO
    {
        public Guid Id { get; set; }
        public InfluencerStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
