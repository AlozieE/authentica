using System.Text.Json;
using Authentica.BLL.DTOs;
using Authentica.BLL.Interfaces;
using Authentica.BLL.Models;

namespace Authentica.BLL.Services
{
    public class VaultItemService : IVaultItemService
    {
        private readonly IVaultItemRepository _vaultItemRepository;
        private readonly IEncryptionService _encryptionService;

        public VaultItemService(IVaultItemRepository vaultItemRepository, IEncryptionService encryptionService)
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

        public async Task CreateItem(int vaultId, string userId, VaultItemCreateDto dto)
        {
            var item = new VaultItem
            {
                Title = dto.Title,
                ItemType = dto.ItemType,
                VaultId = vaultId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(dto.Fields);
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
