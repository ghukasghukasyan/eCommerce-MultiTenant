using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Responses;

namespace ClientLibrary.Helpers.Interface
{
    public interface IApiCallHelper
    {
        Task<HttpResponseMessage> ApiCallTypeCall<TModel>(ApiCall apiCall);
        Task<TResponse> GetServiceResponse<TResponse>(HttpResponseMessage message);
        ServiceResponse<Guid> ConnectionError();
    }
}
