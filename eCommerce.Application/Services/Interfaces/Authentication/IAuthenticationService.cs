using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        Task<ServiceResponse> RegisterAsync(RegisterUserDTO user);
        Task<ServiceResponse> RegisterInfluencerAsync(RegisterInfluencerDTO influencer);
        Task<LoginResponse> LoginUserAsync(LoginUserDTO login);
        Task<LoginResponse> ReviveTokenAsync(string refreshToken);
        Task<ServiceResponse> ConfirmEmailAsync(string userId, string token);
        Task<ServiceResponse> ForgotPasswordAsync(string email);
        Task<ServiceResponse> ResetPasswordAsync(ResetPasswordDTO dto);
        Task LogoutAsync(string userId);
    }
}
