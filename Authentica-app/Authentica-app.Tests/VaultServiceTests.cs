using Authentica_app.BLL.Models;
using Authentica_app.BLL.Services;
using Authentica_app.DAL.Interfaces;
using Moq;

namespace Authentica_app.Tests;

[TestClass]
public class VaultServiceTests
{
    private Mock<IVaultRepository> _mockRepo = null!;
    private VaultService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IVaultRepository>();
        _service = new VaultService(_mockRepo.Object);
    }

    [TestMethod]
    public async Task GetUserVaults_ReturnsOnlyVaultsForCurrentUser()
    {
        var vaults = new List<Vault>
        {
            new Vault { VaultId = 1, Name = "Persoonlijk", UserId = "user1" },
            new Vault { VaultId = 2, Name = "Werk",     UserId = "user1" }
        };
        _mockRepo.Setup(r => r.GetAll("user1")).ReturnsAsync(vaults);

        var result = await _service.GetUserVaults("user1");

        Assert.HasCount(2, result);
        Assert.IsTrue(result.All(v => v.UserId == "user1"));
    }

    [TestMethod]
    public async Task CreateVault_CallsRepositoryAdd()
    {
        var vault = new Vault { VaultId = 1, Name = "Savings", UserId = "user1" };
        _mockRepo.Setup(r => r.Add(vault)).Returns(Task.CompletedTask);

        await _service.CreateVault(vault);

        _mockRepo.Verify(r => r.Add(vault), Times.Once);
    }

    [TestMethod]
    public async Task DeleteVault_CallsRepositoryDelete()
    {
        var vault = new Vault { VaultId = 3, Name = "Oud", UserId = "user1" };
        _mockRepo.Setup(r => r.Delete(vault)).Returns(Task.CompletedTask);

        await _service.DeleteVault(vault);

        _mockRepo.Verify(r => r.Delete(vault), Times.Once);
    }

    [TestMethod]
    public async Task GetById_ReturnsNull_WhenVaultNotFound()
    {
        _mockRepo.Setup(r => r.GetById(99, "user1")).ReturnsAsync((Vault?)null);

        var result = await _service.GetById(99, "user1");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetById_ReturnsVault_WhenFoundAndOwned()
    {
        var vault = new Vault { VaultId = 5, Name = "Reizen", UserId = "user1" };
        _mockRepo.Setup(r => r.GetById(5, "user1")).ReturnsAsync(vault);

        var result = await _service.GetById(5, "user1");

        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.VaultId);
        Assert.AreEqual("user1", result.UserId);
    }
}
