using Authentica.BLL.DTOs;
using Authentica.BLL.Interfaces;
using Authentica.BLL.Services;
using Authentica.DAL.Interfaces;
using Authentica.DAL.Models;
using Moq;

namespace Authentica_app.Tests;

[TestClass]
public class VaultItemServiceTests
{
    private Mock<IVaultItemRepository> _mockRepo = null!;
    private Mock<IEncryptionService> _mockEncryption = null!;
    private VaultItemService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IVaultItemRepository>();

        _mockEncryption = new Mock<IEncryptionService>();
        _mockEncryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns(("fake-ciphertext", "fake-iv"));
        _mockEncryption.Setup(e => e.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns("my secret password");
        _mockEncryption.Setup(e => e.GenerateIV()).Returns("fake-iv");

        _service = new VaultItemService(_mockRepo.Object, _mockEncryption.Object);
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

    [TestMethod]
    public async Task CreateItem_CallsRepositoryAdd()
    {
        var dto = new VaultItemCreateDto
        {
            Title = "Test Item",
            ItemType = VaultItemType.Password,
            Fields = new Dictionary<string, string> { ["username"] = "user", ["password"] = "pass" }
        };
        _mockRepo.Setup(r => r.Add(It.IsAny<VaultItem>())).Returns(Task.CompletedTask);

        await _service.CreateItem(1, "user1", dto);

        _mockRepo.Verify(r => r.Add(It.IsAny<VaultItem>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteItem_CallsRepositoryDelete()
    {
        var item = new VaultItem
        {
            VaultItemId = 1,
            Title = "To Delete",
            VaultId = 1,
            EncryptedData = "",
            IV = ""
        };
        _mockRepo.Setup(r => r.Delete(It.IsAny<VaultItem>())).Returns(Task.CompletedTask);

        await _service.DeleteItem(item);

        _mockRepo.Verify(r => r.Delete(It.IsAny<VaultItem>()), Times.Once);
    }

    [TestMethod]
    public async Task GetById_ReturnsNull_WhenItemNotFound()
    {
        _mockRepo.Setup(r => r.GetById(99)).ReturnsAsync((VaultItem?)null);

        var result = await _service.GetById(99);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetById_ReturnsItem_WhenFound()
    {
        var expected = new VaultItem
        {
            VaultItemId = 5,
            Title = "Found Item",
            VaultId = 1,
            EncryptedData = "",
            IV = ""
        };
        _mockRepo.Setup(r => r.GetById(5)).ReturnsAsync(expected);

        var result = await _service.GetById(5);

        Assert.IsNotNull(result);
        Assert.AreEqual(expected.VaultItemId, result.VaultItemId);
        Assert.AreEqual(expected.Title, result.Title);
    }
}