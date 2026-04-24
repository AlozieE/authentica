using Authentica_app.BLL.Models;
using Authentica_app.DAL.Database;
using Microsoft.EntityFrameworkCore;

namespace Authentica_app.DAL.Repositories
{
    public class VaultItemRepository
    {
        private readonly ApplicationDbContext _context;

        public VaultItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VaultItem>> GetByVault(int vaultId)
        {
            return await _context.VaultItems
                .Where(i => i.VaultId == vaultId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VaultItem>> GetByUserAndType(string userId, VaultItemType type, int? vaultId)
        {
            var query = _context.VaultItems
                .Include(i => i.Vault)
                .Where(i => i.Vault.UserId == userId && i.ItemType == type);

            if (vaultId.HasValue)
                query = query.Where(i => i.VaultId == vaultId.Value);

            return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        public async Task<VaultItem?> GetById(int id)
        {
            return await _context.VaultItems
                .Include(i => i.Vault)
                .FirstOrDefaultAsync(i => i.VaultItemId == id);
        }

        public async Task Add(VaultItem item)
        {
            _context.VaultItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Delete(VaultItem item)
        {
            _context.VaultItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
