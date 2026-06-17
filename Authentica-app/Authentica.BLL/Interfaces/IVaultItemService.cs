using Authentica.BLL.DTOs;
using Authentica.BLL.Models;

namespace Authentica.BLL.Interfaces
{
    public interface IVaultItemService
    {
        Task<List<VaultItem>> GetItems(string userId, int vaultItemTypeId, int? vaultId);
        Task<List<VaultItem>> GetItems(int vaultId);
        Task<VaultItem?> GetById(int id);
        Task CreateItem(int vaultId, string userId, VaultItemCreateDto dto);
        Task UpdateItem(VaultItem item, Dictionary<string, string> data);
        Task DeleteItem(VaultItem item);
        Dictionary<string, string> DecryptItemData(VaultItem item);
    }
}
