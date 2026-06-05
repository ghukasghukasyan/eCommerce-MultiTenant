using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.Addresses;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Responses;
using ClientLibrary.Models.Users;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class UserService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IUserService
    {
        public async Task<ServiceResponse<Guid>> CreateAddress(CreateAddressDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.Address,
                Type = ApiCallType.Post,
                Client = client,
                Model = dto!,
            };

            var result = await apiHelper.ApiCallTypeCall<CreateAddressDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> UpdateAddress(UpdateAddressDTO dto)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.Address,
                Type = ApiCallType.Update,
                Client = client,
                Model = dto!,
            };
            
            var result = await apiHelper.ApiCallTypeCall<UpdateAddressDTO>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse> UpdateProfile(UserProfileDTO user)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.Profile,
                Type = ApiCallType.Update,
                Client = client,
                Model = user!,
            };

            var result = await apiHelper.ApiCallTypeCall<UserProfileDTO>(apiCall);
            if (result == null) return new ServiceResponse(false, "Connection error.");
            if (!result.IsSuccessStatusCode) return new ServiceResponse(false, "Update failed.");
            return await apiHelper.GetServiceResponse<ServiceResponse>(result);
        }
        public async Task<ServiceResponse<Guid>> DeleteAddress(Guid id)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.Address,
                Type = ApiCallType.Delete,
                Client = client,
                Model = null!,
            };

            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            return result == null ? apiHelper.ConnectionError() : await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<UserProfileDTO> GetProfile()
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.Profile,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result == null || !result.IsSuccessStatusCode) return null!;
            return await apiHelper.GetServiceResponse<UserProfileDTO>(result);
        }
        public async Task<List<AddressDTO>> GetUserAddresses()
        {
           var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.Address,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };

            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result == null || !result.IsSuccessStatusCode) return [];
            return await apiHelper.GetServiceResponse<List<AddressDTO>>(result);
        }
    }
}
