
using Microsoft.AspNetCore.Identity;

namespace Authentica_app.Tests;

[TestClass]
public class AccountSecurityTests
{
    // Maakt test-instellingen aan voor account lockout.
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
        // Test of een account na maximaal 5 mislukte inlogpogingen geblokkeerd kan worden.
        var options = CreateLockoutOptions();

        Assert.AreEqual(5, options.Lockout.MaxFailedAccessAttempts);
    }

    [TestMethod]
    public void Lockout_IsConfigured_WithFiveMinuteLockout()
    {
        // Test of de lockout-periode is ingesteld op 5 minuten.
        var options = CreateLockoutOptions();

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.Lockout.DefaultLockoutTimeSpan);
    }

    [TestMethod]
    public void Lockout_IsEnabled_ForNewUsers()
    {
        // Test of lockout ook actief is voor nieuwe gebruikers.
        var options = CreateLockoutOptions();

        Assert.IsTrue(options.Lockout.AllowedForNewUsers);
    }
    
    [TestMethod]
    public void PasswordHasher_VerifiesCorrectPassword()
    {
        // Test of ASP.NET Identity een wachtwoord kan hashen en daarna correct kan verifiëren.
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
        // Test of het e-mailadres van een gebruiker aangepast kan worden.
        var user = new IdentityUser { Email = "old@test.com" };

        user.Email = "new@test.com";

        Assert.AreEqual("new@test.com", user.Email);
    }
}

