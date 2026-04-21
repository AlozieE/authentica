using Authentica_app.Models;
using Authentica_app.Repositories;

namespace Authentica_app.Services
{
    public class VaultService
    {
        private readonly VaultRepository _vaultRepository;

        public VaultService(VaultRepository vaultRepository)
        {
            _vaultRepository = vaultRepository;
        }

        public async Task<List<Vault>> GetUserVaults(string userId)
        {
            return await _vaultRepository.GetAll(userId);
        }

        public async Task<Vault?> GetById(int id, string userId)
        {
            return await _vaultRepository.GetById(id, userId);
        }

        public async Task CreateVault(Vault vault)
        {
            await _vaultRepository.Add(vault);
        }

        public async Task UpdateVault(Vault vault)
        {
            await _vaultRepository.Update(vault);
        }

        public async Task DeleteVault(int id, string userId)
        {
            await _vaultRepository.Delete(id, userId);
        }
    }
}