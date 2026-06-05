using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Authentication
{
    public class ForgotPasswordDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
