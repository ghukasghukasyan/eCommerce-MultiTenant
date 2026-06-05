using eCommerce.Application.DTOs.Bases;

namespace eCommerce.Application.DTOs.Identity
{
    public class RegisterUserDTO : IdentityBaseDTO
    {
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
