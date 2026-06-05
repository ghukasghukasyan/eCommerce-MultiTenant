using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Dashboard;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class DashboardService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IDashboardService
    {
        public async Task<DashboardStatsDTO?> GetStatsAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Dashboard.GetStats,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result != null && result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<DashboardStatsDTO>(result);
            return null;
        }
    }
}
