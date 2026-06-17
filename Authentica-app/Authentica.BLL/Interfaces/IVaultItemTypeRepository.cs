using Authentica.BLL.Models;

namespace Authentica.BLL.Interfaces
{
    public interface IVaultItemTypeRepository
    {
        IEnumerable<VaultItemType> GetAll();
        VaultItemType GetById(int id);
    }
}
