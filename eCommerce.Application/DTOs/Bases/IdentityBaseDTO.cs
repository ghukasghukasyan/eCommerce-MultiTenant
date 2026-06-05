namespace eCommerce.Application.DTOs.Bases
{
    public class IdentityBaseDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
