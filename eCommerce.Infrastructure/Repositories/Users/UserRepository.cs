using eCommerce.Domain.Entities.Addresses;
using eCommerce.Domain.Interfaces.Users;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Users
{
    public class UserRepository(ECommerceContext context) : IUserRepository
    {
        public async Task CreateAddressAsync(UserAddress address)   
        {
            await context.AddAsync(address);
        }

        public Task UpdateAsync(UserAddress address)
        {
            context.UserAddresses.Update(address);
            return Task.CompletedTask;

        }

        public Task DeleteAsync(UserAddress address)
        {
            context.UserAddresses.Remove(address);
            return Task.CompletedTask;
        }

        public async Task<List<UserAddress>> GetUserAddressesAsync(string userId)
        {
            return await context.UserAddresses
                .Where(u => u.UserId == userId).ToListAsync();
        }

        public async Task<UserAddress> GetUserAddressAsync(string userId, Guid id)
        {
            return await context.UserAddresses
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Id == id);
        }

        public async Task SaveAsync()
                 => await context.SaveChangesAsync();

    }
}
