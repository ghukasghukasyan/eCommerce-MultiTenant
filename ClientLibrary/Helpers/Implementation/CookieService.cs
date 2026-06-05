using ClientLibrary.Helpers.Interface;
using Microsoft.JSInterop;

namespace ClientLibrary.Helpers.Implementation
{
    public class CookieService(IJSRuntime js) : ICookieService
    {
        public async Task<string?> GetAsync(string key)
            => await js.InvokeAsync<string?>("cookieService.get", key);

        public async Task SetAsync(string key, string value, int days, string path)
            => await js.InvokeVoidAsync("cookieService.set", key, value, days, path);

        public async Task RemoveAsync(string key)
            => await js.InvokeVoidAsync("cookieService.remove", key);
    }
}
