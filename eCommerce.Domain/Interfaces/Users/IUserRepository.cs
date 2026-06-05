using eCommerce.Domain.Entities.Addresses;

namespace eCommerce.Domain.Interfaces.Users
{
    public interface IUserRepository
    {
        Task CreateAddressAsync(UserAddress address);
        Task UpdateAsync(UserAddress address);
        Task DeleteAsync(UserAddress address);
        Task<List<UserAddress>> GetUserAddressesAsync(string userId);
        Task<UserAddress> GetUserAddressAsync(string userId, Guid id);
        Task SaveAsync();
    }
}
