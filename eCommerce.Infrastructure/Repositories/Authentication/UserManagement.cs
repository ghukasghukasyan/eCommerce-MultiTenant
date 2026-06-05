using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eCommerce.Infrastructure.Repositories.Authentication
{
    public class UserManagement(IRoleManagement roleManagement, UserManager<AppUser> userManager, ECommerceContext context) : IUserManagement
    {
        public async Task<IdentityResult> RegisterAsync(AppUser user, string password)
        {
            var existingUser = await userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                if (!existingUser.EmailConfirmed)
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UnconfirmedUserExists",
                        Description = "UnconfirmedUserExists"
                    });

                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserExists",
                    Description = "User with this email already exists."
                });
            }

            return await userManager.CreateAsync(user, password);
        }

        public async Task<IEnumerable<AppUser>> GetAllAsync() => await context.Users.ToListAsync();

        public async Task<int> CountAsync() => await context.Users.CountAsync();

        public async Task<bool> AnyOtherUserExistsAsync(string excludeUserId)
            => await context.Users.AnyAsync(u => u.Id != excludeUserId);

        public async Task<AppUser> GetByEmailAsync(string email) => await userManager.FindByEmailAsync(email);

        public async Task<AppUser> GetByIdAsync(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            return user!;
        }

        public async Task<List<Claim>> GetClaimsAsync(string email)
        {
            var user = await GetByEmailAsync(email);
            var roles = await roleManagement.GetUserRoles(user!.Email!);

            List<Claim> claims = [
                new Claim("Fullname", user.FullName!),
                new Claim(ClaimTypes.Name, user.FullName!),
                new Claim(ClaimTypes.NameIdentifier, user!.Id!),
                new Claim(ClaimTypes.Email, user!.Email!),
            ];

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            return claims;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var _user = await GetByEmailAsync(email);
            if (_user is null) return false;

            if (!_user.EmailConfirmed) return false;

            var roles = await roleManagement.GetUserRoles(_user.Email!);
            if (roles.Count == 0) return false;

            return await userManager.CheckPasswordAsync(_user, password);
        }

        public async Task<int> RemoveByEmailAsync(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(_ => _.Email == email);
            if (user is null) return 0;

            var influencer = await context.Influencers.FirstOrDefaultAsync(i => i.UserId == user.Id);
            if (influencer != null)
                context.Influencers.Remove(influencer);

            context.Users.Remove(user);
            return await context.SaveChangesAsync();
        }

        public async Task<IdentityResult> UpdateAsync(AppUser appUser)
        {
            return await userManager.UpdateAsync(appUser);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(AppUser user)
            => userManager.GenerateEmailConfirmationTokenAsync(user);

        public Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token)
            => userManager.ConfirmEmailAsync(user, token);

        public Task<string> GeneratePasswordResetTokenAsync(AppUser user)
            => userManager.GeneratePasswordResetTokenAsync(user);

        public Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
            => userManager.ResetPasswordAsync(user, token, newPassword);
    }
}
