using ClientLibrary.Constants;
using ClientLibrary.Helpers.Interface;

namespace ClientLibrary.Helpers.Implementation
{
    public class HttpClientHelper(IHttpClientFactory clientFacory, ITokenService tokenService, ILanguageService languageService) : IHttpClientHelper
    {
        public async Task<HttpClient> GetPrivateClientAsync()
        {
            await languageService.InitializeAsync();

            HttpClient client = clientFacory.CreateClient(AppConstants.ApiClient.Name);
            SetLanguageHeader(client);

            string token = await tokenService.GetJWTTokenAsync();
            if (string.IsNullOrEmpty(token))
                return client;

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AppConstants.Authentication.Type, token);
            return client;
        }

        public async Task<HttpClient> GetPublicClientAsync()
        {
            await languageService.InitializeAsync();
            HttpClient client = clientFacory.CreateClient(AppConstants.ApiClient.Name);
            SetLanguageHeader(client);
            return client;
        }

        private void SetLanguageHeader(HttpClient client)
        {
            client.DefaultRequestHeaders.Remove("Accept-Language");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", languageService.CurrentLanguage);
        }
    }
}
