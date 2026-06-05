using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Authentication;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;
using System.Web;
using static ClientLibrary.Constants.AppConstants;
using static ClientLibrary.Constants.AppConstants.ApiCallType;

namespace ClientLibrary.Services.Implementation
{
    public class AuthenticationService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IAuthenticationService
    {
        public async Task<ServiceResponse<Guid>> RegisterUser(RegisterUserDTO user)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.Register,
                Type = ApiCallType.Post,
                Client = client,
                Id = null!,
                Model = user,
            };

            var result = await apiHelper.ApiCallTypeCall<RegisterUserDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> RegisterInfluencer(RegisterInfluencerDTO influencer)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.RegisterInfluencer,
                Type = ApiCallType.Post,
                Client = client,
                Id = null!,
                Model = influencer,
            };

            var result = await apiHelper.ApiCallTypeCall<RegisterInfluencerDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<LoginResponse> LoginUser(LoginUserDTO loginUser)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.Login,
                Type = ApiCallType.Post,
                Client = client,
                Id = null!,
                Model = loginUser,
            };

            var result = await apiHelper.ApiCallTypeCall<LoginUserDTO>(apiCall);
            return result == null ? new LoginResponse(Message: apiHelper.ConnectionError().Message) : await apiHelper.GetServiceResponse<LoginResponse>(result);
        }
        public async Task<LoginResponse> ReviveToken(string refreshToken)
        {
            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.ReviveToken,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = HttpUtility.UrlEncode(refreshToken),
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            return result == null ? null! : await apiHelper.GetServiceResponse<LoginResponse>(result);
        }

        public async Task<ServiceResponse<Guid>> ConfirmEmail(string userId, string token)
        {
            var client = await httpClient.GetPublicClientAsync();
            var route = $"{Authentication.ConfirmEmail}?userId={HttpUtility.UrlEncode(userId)}&token={HttpUtility.UrlEncode(token)}";
            var apiCall = new ApiCall
            {
                Route = route,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<ServiceResponse<Guid>> ForgotPassword(ForgotPasswordDTO dto)
        {
            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.ForgotPassword,
                Type = ApiCallType.Post,
                Client = client,
                Model = dto,
                Id = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<ForgotPasswordDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<ServiceResponse<Guid>> ResetPassword(ResetPasswordDTO dto)
        {
            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.ResetPassword,
                Type = ApiCallType.Post,
                Client = client,
                Model = dto,
                Id = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<ResetPasswordDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task LogoutAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Authentication.Logout,
                Type = ApiCallType.Post,
                Client = client,
                Model = new { },
                Id = null!,
            };
            // Fire and forget — if it fails we still clear local tokens
            try { await apiHelper.ApiCallTypeCall<object>(apiCall); } catch { }
        }
    }
}
