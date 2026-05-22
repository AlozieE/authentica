using Authentica_app.BLL.Models;
using Authentica_app.BLL.Services;
using Authentica_app.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Authentica_app.Tests;

[TestClass]
public class VaultItemServiceTests_Mocked
{
    private const string TestKey = "X2CBejMWkOPJSWJ6JLDqa56qLo3/PFUdVD0KLrsm+lU=";

    private Mock<IVaultItemRepository> _mockRepo = null!;
    private VaultItemService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IVaultItemRepository>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Encryption:Key"] = TestKey })
            .Build();
        var encryptionService = new EncryptionService(config);

        _service = new VaultItemService(_mockRepo.Object, encryptionService);
    }

    [TestMethod]
    public async Task GetItems_ReturnsItemsForCorrectVault()
    {
        var expected = new List<VaultItem>
        {
            new() { VaultItemId = 1, Title = "Item 1", VaultId = 1, EncryptedData = "", IV = "" },
            new() { VaultItemId = 2, Title = "Item 2", VaultId = 1, EncryptedData = "", IV = "" }
        };
        _mockRepo.Setup(r => r.GetByVault(1)).ReturnsAsync(expected);

        var result = await _service.GetItems(1);

        Assert.HasCount(2, result);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public async Task CreateItem_CallsRepositoryAdd()
    {
        var item = new VaultItem
        {
            Title = "Test Item",
            VaultId = 1,
            ItemType = VaultItemType.Password,
            EncryptedData = "",
            IV = ""
        };
        var data = new Dictionary<string, string> { ["username"] = "user", ["password"] = "pass" };
        _mockRepo.Setup(r => r.Add(It.IsAny<VaultItem>())).Returns(Task.CompletedTask);

        await _service.CreateItem(item, data);

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
