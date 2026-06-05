using ClientLibrary.Constants;
using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;

namespace eCommerce.Frontend.Authentication
{
    public class RefreshTokenHandler(ITokenService tokenService, IAuthenticationService authenticationService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await base.SendAsync(request, cancellationToken);

            if (result.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                return result;

            var refreshToken = await tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
                return result;

            var loginResponse = await MakeApiCall(refreshToken);
            if (loginResponse == null)
                return result;

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                AppConstants.Authentication.Type, loginResponse.Token);
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<LoginResponse> MakeApiCall(string refreshToken)
        {
            var result = await authenticationService.ReviveToken(refreshToken);
            if (result is null) return null!;
            if (result.Success)
            {
                await tokenService.RemoveTokensAsync();
                await tokenService.SetTokensAsync(result.Token, result.RefreshToken, AppConstants.Cookie.CookieExpirationTime, AppConstants.Cookie.Path);
                return result;
            }
            return null!;
        }
    }
}
