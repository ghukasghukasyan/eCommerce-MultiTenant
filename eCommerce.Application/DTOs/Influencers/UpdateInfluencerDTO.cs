namespace eCommerce.Application.DTOs.Influencers
{
    public class UpdateInfluencerDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int DefaultCommissionRate { get; set; }
    }
}
