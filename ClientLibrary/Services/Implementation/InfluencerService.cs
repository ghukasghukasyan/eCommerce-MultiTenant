using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Influencers;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;
using Microsoft.AspNetCore.Http;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class InfluencerService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IInfluencerService
    {
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateInfluencerDTO request)
        {
            var privateClient = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.Create,
                Type = ApiCallType.Post,
                Client = privateClient,
                Id = null!,
                Model = request,
            };
            var result = await apiHelper.ApiCallTypeCall<CreateInfluencerDTO>(apiCall);
            if (result == null)
                return apiHelper.ConnectionError();
            else
                return await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateInfluencerDTO request)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.Update,
                Type = ApiCallType.Update,
                Client = client,
                Id = null!,
                Model = request,
            };

            var result = await apiHelper.ApiCallTypeCall<UpdateInfluencerDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateInfluencerStatusDTO request)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.UpdateStatus,
                Type = ApiCallType.Update,
                Client = client,
                Id = null!,
                Model = request
            };

            var result = await apiHelper.ApiCallTypeCall<UpdateInfluencerStatusDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<IReadOnlyList<GetInfluencerDTO>> GetAllAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.GetAll,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<IReadOnlyList<GetInfluencerDTO>>(result);
            else
                return [];
        }

        public async Task<GetInfluencerDTO> GetByIdAsync(Guid influencerId)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.Get,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };
            apiCall.ToString(influencerId);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<GetInfluencerDTO>(result);
            else
                return null!;
        }

        public async Task<GetInfluencerDTO> GetMeAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.Me,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<GetInfluencerDTO>(result);
            return null!;
        }

        public async Task<InfluencerStatsDTO?> GetStatsAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Influencer.Stats,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<InfluencerStatsDTO>(result);
            return null;
        }

        public async Task<InfluencerStatsDTO?> GetStatsByIdAsync(Guid influencerId)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = $"{Influencer.StatsByIdBase}/{influencerId}/stats",
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<InfluencerStatsDTO>(result);
            return null;
        }

        public async Task<ServiceResponse> RecordPayoutAsync(Guid influencerId, decimal amount, string? note)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var payload = new { Amount = amount, Note = note };
            var apiCall = new ApiCall
            {
                Route = $"{Influencer.StatsByIdBase}/{influencerId}/payout",
                Type = ApiCallType.Post,
                Client = client,
                Model = payload,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<object>(apiCall);
            if (result == null) return new ServiceResponse(false, "Connection error.");
            return await apiHelper.GetServiceResponse<ServiceResponse>(result);
        }

        public Task UploadAvatarAsync(Guid influencerId, IFormFile file)
        {
            throw new NotImplementedException();
        }
    }
}
