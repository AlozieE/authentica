using System.Text.Json;
using Authentica_app.BLL.Interfaces;
using Authentica_app.BLL.Models;
using Authentica_app.DAL.Interfaces;

namespace Authentica_app.BLL.Services
{
    public class VaultItemService : IVaultItemService
    {
        private readonly IVaultItemRepository _vaultItemRepository;
        private readonly EncryptionService _encryptionService;

        public VaultItemService(IVaultItemRepository vaultItemRepository, EncryptionService encryptionService)
        {
            _vaultItemRepository = vaultItemRepository;
            _encryptionService = encryptionService;
        }

        public async Task<List<VaultItem>> GetItems(int vaultId)
            => await _vaultItemRepository.GetByVault(vaultId);

        public async Task<List<VaultItem>> GetItems(string userId, VaultItemType type, int? vaultId = null)
            => await _vaultItemRepository.GetByUserAndType(userId, type, vaultId);

        public async Task<VaultItem?> GetById(int id)
            => await _vaultItemRepository.GetById(id);

        public async Task CreateItem(VaultItem item, Dictionary<string, string> data)
        {
            var json = JsonSerializer.Serialize(data);
            (item.EncryptedData, item.IV) = _encryptionService.Encrypt(json);
            await _vaultItemRepository.Add(item);
        }

        public async Task UpdateItem(VaultItem item, Dictionary<string, string> data)
        {
            var json = JsonSerializer.Serialize(data);
            (item.EncryptedData, item.IV) = _encryptionService.Encrypt(json);
            item.UpdatedAt = DateTime.UtcNow;
            await _vaultItemRepository.Update(item);
        }

        public async Task DeleteItem(VaultItem item)
            => await _vaultItemRepository.Delete(item);

        public Dictionary<string, string> DecryptItemData(VaultItem item)
        {
            var json = _encryptionService.Decrypt(item.EncryptedData, item.IV);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
    }
}
