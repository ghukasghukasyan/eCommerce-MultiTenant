using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Responses;
using ClientLibrary.Models.Variants;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class VariantService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IVariantService
    {
        public async Task<List<VariantDTO>> GetByProductIdAsync(Guid productId)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var response = await client.GetAsync($"{Variant.GetByProduct}/{productId}");
            if (!response.IsSuccessStatusCode)
                return new();
            return await apiHelper.GetServiceResponse<List<VariantDTO>>(response);
        }

        public async Task<ServiceResponse> GenerateAsync(GenerateVariantsDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Variant.Generate,
                Type = ApiCallType.Post,
                Client = client,
                Model = dto
            };
            var result = await apiHelper.ApiCallTypeCall<GenerateVariantsDTO>(apiCall);
            if (result == null) return new ServiceResponse(false, apiHelper.ConnectionError().Message);
            return await apiHelper.GetServiceResponse<ServiceResponse>(result);
        }

        public async Task<ServiceResponse> UpdateVariantAsync(UpdateVariantDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Variant.Update,
                Type = ApiCallType.Update,
                Client = client,
                Model = dto
            };
            var result = await apiHelper.ApiCallTypeCall<UpdateVariantDTO>(apiCall);
            if (result == null) return new ServiceResponse(false, apiHelper.ConnectionError().Message);
            return await apiHelper.GetServiceResponse<ServiceResponse>(result);
        }
    }
}
