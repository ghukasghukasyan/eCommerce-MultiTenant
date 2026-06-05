using ClientLibrary.Constants;
using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Orders;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class PaymentMethodService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IPaymentMethodService
    {
        public async Task<IEnumerable<GetPaymentMethodDTO>> GetPaymentMethods()
        {
            var client = await httpClient.GetPublicClientAsync();
            var apiCall = new ApiCall
            {
                Route = AppConstants.Payment.GetAll,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<IEnumerable<GetPaymentMethodDTO>>(result);
            else
                return [];
        }
    }
}
