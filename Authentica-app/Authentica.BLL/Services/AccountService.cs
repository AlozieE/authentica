using Authentica.BLL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Authentica.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword) =>
            _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        public async Task<IdentityResult> ChangeEmailAsync(IdentityUser user, string newEmail)
        {
            var result = await _userManager.SetEmailAsync(user, newEmail);
            if (!result.Succeeded)
                return result;
            return await _userManager.SetUserNameAsync(user, newEmail);
        }
    }
}
