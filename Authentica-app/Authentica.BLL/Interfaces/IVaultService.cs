using Authentica.BLL.DTOs;
using Authentica.BLL.Models;

namespace Authentica.BLL.Interfaces
{
    public interface IVaultService
    {
        Task<List<Vault>> GetUserVaults(string userId);
        Task<Vault?> GetById(int id, string userId);
        Task CreateVault(string userId, VaultCreateDto dto);
        Task UpdateVault(Vault vault);
        Task DeleteVault(Vault vault);
    }
}
