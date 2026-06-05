using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Identity
{
    public class ForgotPasswordDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
