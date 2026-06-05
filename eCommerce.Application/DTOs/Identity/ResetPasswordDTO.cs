using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Identity
{
    public class ResetPasswordDTO
    {
        [Required] public string UserId  { get; set; } = string.Empty;
        [Required] public string Token   { get; set; } = string.Empty;
        [Required] public string NewPassword { get; set; } = string.Empty;
    }
}
