using ClientLibrary.Constants;
using ClientLibrary.Helpers.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace eCommerce.Frontend.Authentication
{
    public class CustomAuthStateProvider(ITokenService tokenService) : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string jwt = await tokenService.GetJWTTokenAsync();
                if (string.IsNullOrEmpty(jwt))
                    return await Task.FromResult(new AuthenticationState(_anonymous));

                var claims = GetClaims(jwt);
                if (!claims.Any())
                {
                    await tokenService.RemoveTokensAsync();
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var claimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims,"jwtAuth"));
                return await Task.FromResult(new AuthenticationState(claimPrincipal));
            }
            catch 
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

        }
        public void NotifyAuthenticationState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        private static IEnumerable<Claim> GetClaims(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            if (token.ValidTo < DateTime.UtcNow)
                return [];
            return [.. token.Claims];
        }
    }
}
