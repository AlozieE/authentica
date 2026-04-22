using Authentica_app.Data;
using Authentica_app.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentica_app.Repositories
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

        public async Task Update(VaultItem item)
        {
            _context.VaultItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await GetById(id);
            if (item != null)
            {
                _context.VaultItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}