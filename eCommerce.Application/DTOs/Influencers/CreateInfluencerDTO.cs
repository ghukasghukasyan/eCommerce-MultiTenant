using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Influencers
{
    public class CreateInfluencerDTO
    {
        [Required]
        public required string FullName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        [Range(0, 100)]
        public int DefaultCommissionRate { get; set; }
    }
}
