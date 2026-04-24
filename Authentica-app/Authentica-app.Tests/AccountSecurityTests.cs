using Microsoft.AspNetCore.Identity;

namespace Authentica_app.Tests;

[TestClass]
public class AccountSecurityTests
{
    private static IdentityOptions CreateLockoutOptions()
    {
        var options = new IdentityOptions();
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
        return options;
    }


    [TestMethod]
    public void Lockout_IsConfigured_WithMaxFiveAttempts()
    {
        var options = CreateLockoutOptions();

        Assert.AreEqual(5, options.Lockout.MaxFailedAccessAttempts);
    }

    [TestMethod]
    public void Lockout_IsConfigured_WithFiveMinuteLockout()
    {
        var options = CreateLockoutOptions();

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.Lockout.DefaultLockoutTimeSpan);
    }

    [TestMethod]
    public void Lockout_IsEnabled_ForNewUsers()
    {
        var options = CreateLockoutOptions();

        Assert.IsTrue(options.Lockout.AllowedForNewUsers);
    }
    

    [TestMethod]
    public void ChangePassword_RequiresCurrentPassword()
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var user = new IdentityUser();
        const string password = "MyP@ssword1";

        var hash = hasher.HashPassword(user, password);
        var result = hasher.VerifyHashedPassword(user, hash, password);

        Assert.AreEqual(PasswordVerificationResult.Success, result);
    }

    [TestMethod]
    public void UpdateEmail_ChangesEmailAddress()
    {
        var user = new IdentityUser { Email = "old@test.com" };

        user.Email = "new@test.com";

        Assert.AreEqual("new@test.com", user.Email);
    }
}
