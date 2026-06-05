namespace ClientLibrary.Helpers.Interface
{
    public interface ICookieService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value, int days, string path);
        Task RemoveAsync(string key);
    }
}
