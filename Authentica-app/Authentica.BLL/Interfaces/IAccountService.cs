using Microsoft.AspNetCore.Identity;

namespace Authentica.BLL.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword);
        Task<IdentityResult> ChangeEmailAsync(IdentityUser user, string newEmail);
    }
}
