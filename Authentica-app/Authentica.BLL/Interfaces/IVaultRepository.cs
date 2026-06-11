using Authentica.BLL.Models;

namespace Authentica.BLL.Interfaces
{
    public interface IVaultRepository
    {
        Task<List<Vault>> GetAll(string userId);
        Task<Vault?> GetById(int id, string userId);
        Task Add(Vault vault);
        Task Update(Vault vault);
        Task Delete(Vault vault);
    }
}
