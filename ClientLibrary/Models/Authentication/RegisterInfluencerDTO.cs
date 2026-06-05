using ClientLibrary.Models.Bases;
using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Authentication
{
    public class RegisterInfluencerDTO : AuthenticationBase
    {
        [Required]
        public  string FullName { get; set; }
        [Required]
        public  string PhoneNumber { get; set; }
        [Required]
        public  string InstagramAccountUrl { get; set; }
        [Required]
        public  string TikTokAccountUrl { get; set; }
        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
