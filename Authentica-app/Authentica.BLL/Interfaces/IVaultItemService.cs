using Authentica.DAL.Models;

namespace Authentica.BLL.Interfaces
{
    public interface IVaultItemService
    {
        Task<List<VaultItem>> GetItems(string userId, VaultItemType type, int? vaultId);
        Task<VaultItem?> GetById(int id);
        Task CreateItem(VaultItem item, Dictionary<string, string> data);
        Task UpdateItem(VaultItem item, Dictionary<string, string> data);
        Task DeleteItem(VaultItem item);
        Dictionary<string, string> DecryptItemData(VaultItem item);
    }
}
