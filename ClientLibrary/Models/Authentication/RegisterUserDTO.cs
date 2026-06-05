using ClientLibrary.Models.Bases;
using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Authentication
{
    public class RegisterUserDTO : AuthenticationBase
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public  string PhoneNumber { get; set; }
        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
