using Authentica_app.Models;
using Authentica_app.Repositories;
using System.Text.Json;

namespace Authentica_app.Services
{
    public class VaultItemService
    {
        private readonly VaultItemRepository _vaultItemRepository;
        private readonly EncryptionService _encryptionService;

        public VaultItemService(VaultItemRepository vaultItemRepository, EncryptionService encryptionService)
        {
            _vaultItemRepository = vaultItemRepository;
            _encryptionService = encryptionService;
        }

        public async Task<List<VaultItem>> GetItems(int vaultId)
        {
            return await _vaultItemRepository.GetByVault(vaultId);
        }

        public async Task<VaultItem?> GetById(int id)
        {
            return await _vaultItemRepository.GetById(id);
        }

        public async Task CreateItem(VaultItem item, Dictionary<string, string> data)
        {
            var jsonData = JsonSerializer.Serialize(data);
            var (encryptedData, iv) = _encryptionService.Encrypt(jsonData);
            item.EncryptedData = encryptedData;
            item.IV = iv;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            await _vaultItemRepository.Add(item);
        }

        public async Task UpdateItem(VaultItem item, Dictionary<string, string> data)
        {
            var jsonData = JsonSerializer.Serialize(data);
            var (encryptedData, iv) = _encryptionService.Encrypt(jsonData);
            item.EncryptedData = encryptedData;
            item.IV = iv;
            item.UpdatedAt = DateTime.UtcNow;
            await _vaultItemRepository.Update(item);
        }

        public async Task DeleteItem(int id)
        {
            await _vaultItemRepository.Delete(id);
        }

        public Dictionary<string, string> DecryptItemData(VaultItem item)
        {
            var decryptedJson = _encryptionService.Decrypt(item.EncryptedData, item.IV);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson)!;
        }
    }
}