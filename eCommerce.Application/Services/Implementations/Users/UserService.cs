using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.DTOs.Users;
using eCommerce.Application.Services.Interfaces.Users;
using eCommerce.Domain.Entities.Addresses;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Influencers;
using eCommerce.Domain.Interfaces.Users;

namespace eCommerce.Application.Services.Implementations.Users
{
    public class UserService(IUserManagement userManager, IInfluencerRepository influencerRepository, IUserRepository userRepository) : IUserService
    {
        public async Task<ServiceResponse<Guid>> CreateAddressAsync(string userId, CreateAddressDTO dto)
        {
            if (dto.IsDefault)
            {
                var existingAddresses = await userRepository.GetUserAddressesAsync(userId);

                foreach (var addr in existingAddresses)
                {
                    addr.IsDefault = false;
                    await userRepository.UpdateAsync(addr);
                }
            }

            var address = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                City = dto.City,
                AddressLine = dto.AddressLine,
                Notes = dto.Notes,
                PostalCode = dto.PostalCode,
                IsDefault = dto.IsDefault
            };

            await userRepository.CreateAddressAsync(address);
            await userRepository.SaveAsync();

            return new ServiceResponse<Guid>(true, address.Id, "Successfully created");
        }
        public async Task<ServiceResponse<Guid>> UpdateAddressAsync(string userId, UpdateAddressDTO dto)
        {
            var allAddresses = await userRepository.GetUserAddressesAsync(userId);
            var existingAddress = allAddresses.FirstOrDefault(a => a.Id == dto.Id);

            if (existingAddress == null)
            {
                return new ServiceResponse<Guid>(false, Guid.Empty, "Address not found");
            }

            if (dto.IsDefault)
            {
                foreach (var addr in allAddresses.Where(a => a.Id != dto.Id && a.IsDefault))
                {
                    addr.IsDefault = false;
                }
            }

            existingAddress.FullName = dto.FullName;
            existingAddress.PhoneNumber = dto.PhoneNumber;
            existingAddress.City = dto.City;
            existingAddress.AddressLine = dto.AddressLine;
            existingAddress.PostalCode = dto.PostalCode;
            existingAddress.Notes = dto.Notes;
            existingAddress.IsDefault = dto.IsDefault;

            await userRepository.SaveAsync();

            return new ServiceResponse<Guid>(true, existingAddress.Id, "Successfully updated");
        }
        public async Task<ServiceResponse> UpdateProfileAsync(UserProfileDTO profile)
        {
            var user = await userManager.GetByIdAsync(profile.Id);
            if (user == null)
                return new ServiceResponse(false, "User not found");

            user.FullName    = profile.Name;
            user.PhoneNumber = profile.Phone;
            user.Email       = profile.Email;
            user.UserName    = profile.Email;

            var updateResult = await userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                await UpdateInfluencer(profile);

                return new ServiceResponse(true, "Successfully updated");
            }

            return new ServiceResponse(false, updateResult.Errors.FirstOrDefault()?.Description ?? "Update failed");
        }

        public async Task<ServiceResponse> DeleteAddressAsync(string userId, Guid id)
        {
            var existingAddress = await userRepository.GetUserAddressAsync(userId, id);

            if (existingAddress == null)
            {
                return new ServiceResponse(false, "Not existing address");
            }

            await userRepository.DeleteAsync(existingAddress);
            await userRepository.SaveAsync();

            return new ServiceResponse(true, "Successfully deleted");
        }

        public async Task<UserProfileDTO> GetProfileAsync(string userId)
        {
            var user = await userManager.GetByIdAsync(userId)
                ?? throw new InvalidOperationException($"User '{userId}' not found.");

            return new UserProfileDTO
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber
            };
        }

        public async Task<List<AddressDTO>> GetUserAddressesAsync(string userId)
        {
            var addresses = await userRepository.GetUserAddressesAsync(userId);

            return [.. addresses.Select(x => new AddressDTO
            {
                Id = x.Id,
                FullName = x.FullName,
                PhoneNumber = x.PhoneNumber,
                City = x.City,
                AddressLine = x.AddressLine,
                Notes = x.Notes,
                PostalCode = x.PostalCode,
                IsDefault = x.IsDefault
            })];
        }

        private async Task UpdateInfluencer(UserProfileDTO profile)
        {
            var influencer = await influencerRepository.GetByIdAsync(profile.Id)
                          ?? await influencerRepository.GetByEmailAsync(profile.Email);
            if (influencer == null)
                return;

            influencer.FullName    = profile.Name;
            influencer.PhoneNumber = profile.Phone;
            influencer.Email       = profile.Email;

            await influencerRepository.UpdateAsync(influencer);
        }
    }
}
