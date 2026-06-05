using ClientLibrary.Models.Authentication;
using ClientLibrary.Models.Responses;

namespace ClientLibrary.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<ServiceResponse<Guid>> RegisterUser(RegisterUserDTO user);
        Task<ServiceResponse<Guid>> RegisterInfluencer(RegisterInfluencerDTO influencer);
        Task<LoginResponse> LoginUser(LoginUserDTO loginUser);
        Task<LoginResponse> ReviveToken(string refreshToken);
        Task<ServiceResponse<Guid>> ConfirmEmail(string userId, string token);
        Task<ServiceResponse<Guid>> ForgotPassword(ForgotPasswordDTO dto);
        Task<ServiceResponse<Guid>> ResetPassword(ResetPasswordDTO dto);
        Task LogoutAsync();
    }
}
