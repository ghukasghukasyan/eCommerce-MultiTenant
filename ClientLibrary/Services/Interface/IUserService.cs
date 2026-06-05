using ClientLibrary.Models.Addresses;
using ClientLibrary.Models.Responses;
using ClientLibrary.Models.Users;

namespace ClientLibrary.Services.Interface
{
    public interface IUserService
    {
        Task<ServiceResponse<Guid>> CreateAddress(CreateAddressDTO dto);
        Task<ServiceResponse<Guid>> UpdateAddress(UpdateAddressDTO dto);
        Task<ServiceResponse<Guid>> DeleteAddress(Guid id);
        Task<ServiceResponse> UpdateProfile(UserProfileDTO user);
        Task<UserProfileDTO> GetProfile();
        Task<List<AddressDTO>> GetUserAddresses();
    }
}
