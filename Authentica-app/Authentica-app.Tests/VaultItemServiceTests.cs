using Authentica_app.BLL.Models;
using Authentica_app.BLL.Services;
using Authentica_app.DAL.Database;
using Authentica_app.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
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

    // Each test gets its own isolated in-memory database
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    // ──────────────────────────────────────────────
    // FR9 – EncryptionService tests
    // ──────────────────────────────────────────────

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

    // ──────────────────────────────────────────────
    // FR8 – VaultItem CRUD tests
    // ──────────────────────────────────────────────

    [TestMethod]
    public async Task CreateItem_SavesItemToDatabase()
    {
        using var context = CreateDbContext();
        var service = new VaultItemService(new VaultItemRepository(context), CreateEncryptionService());

        var item = new VaultItem
        {
            Title = "Test Password",
            ItemType = VaultItemType.Password,
            VaultId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var data = new Dictionary<string, string>
        {
            ["Website"] = "https://example.com",
            ["Username"] = "testuser",
            ["Password"] = "secret"
        };

        await service.CreateItem(item, data);

        Assert.AreEqual(1, context.VaultItems.Count());
        Assert.AreEqual("Test Password", context.VaultItems.First().Title);
    }

    [TestMethod]
    public async Task DeleteItem_RemovesItemFromDatabase()
    {
        using var context = CreateDbContext();
        var service = new VaultItemService(new VaultItemRepository(context), CreateEncryptionService());

        var item = new VaultItem
        {
            Title = "To Delete",
            ItemType = VaultItemType.Password,
            VaultId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.VaultItems.Add(item);
        await context.SaveChangesAsync();

        await service.DeleteItem(item);

        Assert.AreEqual(0, context.VaultItems.Count());
    }

    [TestMethod]
    public async Task GetItems_ReturnsOnlyItemsForSpecificVault()
    {
        using var context = CreateDbContext();
        var service = new VaultItemService(new VaultItemRepository(context), CreateEncryptionService());

        context.VaultItems.AddRange(
            new VaultItem { Title = "Vault1-A", ItemType = VaultItemType.Password, VaultId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new VaultItem { Title = "Vault1-B", ItemType = VaultItemType.Password, VaultId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new VaultItem { Title = "Vault2-A", ItemType = VaultItemType.Password, VaultId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var result = await service.GetItems(vaultId: 1);

        Assert.HasCount(2, result);
        Assert.IsTrue(result.All(i => i.VaultId == 1));
    }
}
