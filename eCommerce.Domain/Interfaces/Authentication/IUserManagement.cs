using eCommerce.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace eCommerce.Domain.Interfaces.Authentication
{
    public interface IUserManagement
    {
        Task<IdentityResult> RegisterAsync(AppUser user, string password);
        Task<bool> LoginAsync(string email, string password);
        Task<IdentityResult> UpdateAsync(AppUser appUser);
        Task<AppUser> GetByEmailAsync(string email);
        Task<AppUser> GetByIdAsync(string id);
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<int> CountAsync();
        Task<bool> AnyOtherUserExistsAsync(string excludeUserId);
        Task<int> RemoveByEmailAsync(string email);
        Task<List<Claim>> GetClaimsAsync(string email);

        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
    }
}
