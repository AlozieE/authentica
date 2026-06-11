using Authentica.BLL.Models;

namespace Authentica.BLL.Interfaces
{
    public interface IVaultItemRepository
    {
        Task<List<VaultItem>> GetByVault(int vaultId);
        Task<List<VaultItem>> GetByUserAndType(string userId, VaultItemType type, int? vaultId);
        Task<VaultItem?> GetById(int id);
        Task Add(VaultItem item);
        Task Update(VaultItem item);
        Task Delete(VaultItem item);
    }
}
