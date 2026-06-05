namespace ClientLibrary.Models.Influencers
{
    public class CreateInfluencerDTO
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int DefaultCommissionRate { get; set; }
    }
}
