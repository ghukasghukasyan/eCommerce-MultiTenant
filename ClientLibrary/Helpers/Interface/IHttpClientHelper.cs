namespace ClientLibrary.Helpers.Interface
{
    public interface IHttpClientHelper
    {
        Task<HttpClient> GetPublicClientAsync();
        Task<HttpClient> GetPrivateClientAsync();
    }
}
