namespace ClientLibrary.Helpers.Interface
{
    public interface ITokenService
    {
            Task<string> GetJWTTokenAsync();
        Task<string> GetRefreshTokenAsync();
        Task SetTokensAsync(string jwt, string refreshToken, int days, string path);
        Task RemoveTokensAsync();
    }
}
