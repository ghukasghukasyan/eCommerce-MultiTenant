using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Responses;
using System.Net.Http.Json;

namespace ClientLibrary.Helpers.Implementation
{
    public class ApiCallHelper : IApiCallHelper
    {
        public async Task<HttpResponseMessage> ApiCallTypeCall<TModel>(ApiCall apiCall)
        {
            try
            {
                switch (apiCall.Type)
                {
                    case "post":
                        return await apiCall.Client!.PostAsJsonAsync(apiCall.Route, (TModel)apiCall.Model!);
                    case "update":
                        return await apiCall.Client!.PutAsJsonAsync(apiCall.Route, (TModel)apiCall.Model!);
                    case "delete":
                        return await apiCall.Client!.DeleteAsync($"{apiCall.Route}/{apiCall.Id}");
                    case "get":
                        string idRoute = apiCall.Id != null ? $"/{apiCall.Id}" : null!;
                        return await apiCall.Client!.GetAsync($"{apiCall.Route}{idRoute}");
                    default: throw new Exception("Api call type not specified");
                }
            }
            catch
            {
                throw;
            }
        }

        public ServiceResponse<Guid> ConnectionError()
        {
            return new ServiceResponse<Guid>(false, Guid.Empty, "Error occurred in communicating to the server");
        }

        public async Task<TResponse> GetServiceResponse<TResponse>(HttpResponseMessage message)
        {
            var response = await message.Content.ReadFromJsonAsync<TResponse>()!;
            return response!;
        }
    }
}
