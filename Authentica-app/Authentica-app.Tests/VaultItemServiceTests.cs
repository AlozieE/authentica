using Authentica.BLL.Services;
using Microsoft.Extensions.Configuration;

namespace Authentica_app.Tests;

[TestClass]
public class VaultItemServiceTests
{
    private const string TestKey = "X2CBejMWkOPJSWJ6JLDqa56qLo3/PFUdVD0KLrsm+lU=";

    private static EncryptionService CreateEncryptionService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Encryption:Key"] = TestKey })
            .Build();
        return new EncryptionService(config);
    }

    [TestMethod]
    public void Encrypt_ReturnsEncryptedData_DifferentFromInput()
    {
        var service = CreateEncryptionService();

        var (encryptedData, _) = service.Encrypt("my secret password");

        Assert.AreNotEqual("my secret password", encryptedData);
    }

    [TestMethod]
    public void Decrypt_ReturnsOriginalData_AfterEncryption()
    {
        var service = CreateEncryptionService();
        const string original = "my secret password";

        var (encryptedData, iv) = service.Encrypt(original);
        var decrypted = service.Decrypt(encryptedData, iv);

        Assert.AreEqual(original, decrypted);
    }

    [TestMethod]
    public void Encrypt_GeneratesUniqueIV_ForEachItem()
    {
        var service = CreateEncryptionService();

        var (_, iv1) = service.Encrypt("same password");
        var (_, iv2) = service.Encrypt("same password");

        Assert.AreNotEqual(iv1, iv2);
    }
}
