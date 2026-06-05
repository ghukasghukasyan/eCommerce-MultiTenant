using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.DTOs.Users;

namespace eCommerce.Application.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<ServiceResponse<Guid>> CreateAddressAsync(string userId, CreateAddressDTO dto);
        Task<ServiceResponse<Guid>> UpdateAddressAsync(string userId, UpdateAddressDTO dto);
        Task<ServiceResponse> UpdateProfileAsync(UserProfileDTO profile);
        Task<ServiceResponse> DeleteAddressAsync(string userId, Guid id);
        Task<UserProfileDTO> GetProfileAsync(string userId);
        Task<List<AddressDTO>> GetUserAddressesAsync(string userId);
    }
}
