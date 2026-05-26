using Authentica.BLL.Interfaces;
using Authentica.DAL.Models;
using Authentica.DAL.Interfaces;

namespace Authentica.BLL.Services
{
    public class VaultService : IVaultService
    {
        private readonly IVaultRepository _vaultRepository;

        public VaultService(IVaultRepository vaultRepository)
        {
            _vaultRepository = vaultRepository;
        }

        public async Task<List<Vault>> GetUserVaults(string userId)
            => await _vaultRepository.GetAll(userId);

        public async Task<Vault?> GetById(int id, string userId)
            => await _vaultRepository.GetById(id, userId);

        public async Task CreateVault(Vault vault)
            => await _vaultRepository.Add(vault);

        public async Task UpdateVault(Vault vault)
            => await _vaultRepository.Update(vault);

        public async Task DeleteVault(Vault vault)
            => await _vaultRepository.Delete(vault);
    }
}
