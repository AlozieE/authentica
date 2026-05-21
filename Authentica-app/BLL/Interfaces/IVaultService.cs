using Authentica_app.BLL.Models;

namespace Authentica_app.BLL.Interfaces
{
    public interface IVaultService
    {
        Task<List<Vault>> GetUserVaults(string userId);
        Task<Vault?> GetById(int id, string userId);
        Task CreateVault(Vault vault);
        Task UpdateVault(Vault vault);
        Task DeleteVault(Vault vault);
    }
}
