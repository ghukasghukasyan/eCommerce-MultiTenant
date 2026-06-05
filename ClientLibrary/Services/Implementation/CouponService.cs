using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Coupons;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class CouponService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : ICouponService
    {
        public async Task<List<GetCouponDTO>> GetAllAsync()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Coupon.GetAll,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<List<GetCouponDTO>>(result);
            return [];
        }

        public async Task<List<GetCouponDTO>> GetByInfluencerAsync(Guid influencerId)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = $"{Coupon.GetByInfluencer}/{influencerId}",
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<List<GetCouponDTO>>(result);
            return [];
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateCouponDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Coupon.Create,
                Type = ApiCallType.Post,
                Client = client,
                Model = dto!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<CreateCouponDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateCouponDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Coupon.Update,
                Type = ApiCallType.Update,
                Client = client,
                Model = dto!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<UpdateCouponDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateCouponStatusDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Coupon.UpdateStatus,
                Type = ApiCallType.Update,
                Client = client,
                Model = dto!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<UpdateCouponStatusDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }

        public async Task<CouponValidationResultDTO> ValidateAsync(ValidateCouponDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Coupon.Validate,
                Type = ApiCallType.Post,
                Client = client,
                Model = dto!,
                Id = null!
            };

            var result = await apiHelper.ApiCallTypeCall<ValidateCouponDTO>(apiCall);
            if (result != null && result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<CouponValidationResultDTO>(result);
            return new CouponValidationResultDTO { IsValid = false, Message = "Unable to validate coupon." };
        }
    }
}
