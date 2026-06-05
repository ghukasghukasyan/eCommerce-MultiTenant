using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Responses;
using ClientLibrary.Models.Variants;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class VariantAttributeService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IVariantAttributeService
    {
        public async Task<List<VariantAttributeDTO>> GetAllAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var response = await client.GetAsync(VariantAttribute.GetAll);
            if (!response.IsSuccessStatusCode)
                return new();
            return await apiHelper.GetServiceResponse<List<VariantAttributeDTO>>(response);
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(string name)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = VariantAttribute.Create,
                Type = ApiCallType.Post,
                Client = client,
                Model = new { Name = name }
            };
            var result = await apiHelper.ApiCallTypeCall<object>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
    }
}
