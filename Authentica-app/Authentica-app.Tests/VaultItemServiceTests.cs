using Authentica.BLL.Interfaces;
using Moq;

namespace Authentica_app.Tests;

[TestClass]
public class VaultItemServiceTests
{
    private Mock<IEncryptionService> _mockEncryption = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockEncryption = new Mock<IEncryptionService>();
        _mockEncryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns(("fake-ciphertext", "fake-iv"));
        _mockEncryption.Setup(e => e.Decrypt("fake-ciphertext", "fake-iv")).Returns("my secret password");
        _mockEncryption.Setup(e => e.GenerateIV()).Returns("fake-iv");
    }

    [TestMethod]
    public void Encrypt_ReturnsEncryptedData_DifferentFromInput()
    {
        var (encryptedData, _) = _mockEncryption.Object.Encrypt("my secret password");

        Assert.AreNotEqual("my secret password", encryptedData);
    }

    [TestMethod]
    public void Decrypt_ReturnsOriginalData_AfterEncryption()
    {
        const string original = "my secret password";

        var (encryptedData, iv) = _mockEncryption.Object.Encrypt(original);
        var decrypted = _mockEncryption.Object.Decrypt(encryptedData, iv);

        Assert.AreEqual(original, decrypted);
    }

    [TestMethod]
    public void Encrypt_GeneratesUniqueIV_ForEachItem()
    {
        _mockEncryption.SetupSequence(e => e.Encrypt(It.IsAny<string>()))
            .Returns(("fake-ciphertext-1", "fake-iv-1"))
            .Returns(("fake-ciphertext-2", "fake-iv-2"));

        var (_, iv1) = _mockEncryption.Object.Encrypt("same password");
        var (_, iv2) = _mockEncryption.Object.Encrypt("same password");

        Assert.AreNotEqual(iv1, iv2);
    }
}
