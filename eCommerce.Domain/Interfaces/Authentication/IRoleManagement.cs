using eCommerce.Domain.Entities.Identity;

namespace eCommerce.Domain.Interfaces.Authentication
{
    public interface IRoleManagement
    {
        Task<IList<string>> GetUserRoles(string userEmail);
        Task<bool> AddUserToRole(AppUser user, string roleName);
    }
}
