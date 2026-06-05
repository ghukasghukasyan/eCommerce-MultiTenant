using eCommerce.Application.DTOs.Bases;

namespace eCommerce.Application.DTOs.Identity
{
    public class RegisterInfluencerDTO : IdentityBaseDTO
    {
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string InstagramAccountUrl { get; set; }
        public required string TikTokAccountUrl { get; set; }
    }
}
