using ClientLibrary.Constants;
using ClientLibrary.Helpers.Interface;

namespace ClientLibrary.Helpers.Implementation
{
    public class TokenService(ICookieService cookieService) : ITokenService
    {
        public async Task<string> GetJWTTokenAsync()
            => await cookieService.GetAsync(AppConstants.Cookie.Name) ?? string.Empty;

        public async Task<string> GetRefreshTokenAsync()
            => await cookieService.GetAsync(AppConstants.Cookie.RefreshTokenName) ?? string.Empty;

        public async Task SetTokensAsync(string jwt, string refreshToken, int days, string path)
        {
            await cookieService.SetAsync(AppConstants.Cookie.Name, jwt, days, path);
            await cookieService.SetAsync(AppConstants.Cookie.RefreshTokenName, refreshToken, days, path);
        }

        public async Task RemoveTokensAsync()
        {
            await cookieService.RemoveAsync(AppConstants.Cookie.Name);
            await cookieService.RemoveAsync(AppConstants.Cookie.RefreshTokenName);
        }
    }
}
