using Authentica_app.Data;
using Authentica_app.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentica_app.Repositories
{
    public class VaultRepository
    {
        private readonly ApplicationDbContext _context;

        public VaultRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vault>> GetAll(string userId)
        {
            return await _context.Vaults
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Vault?> GetById(int id, string userId)
        {
            return await _context.Vaults
                .FirstOrDefaultAsync(v => v.VaultId == id && v.UserId == userId);
        }

        public async Task Add(Vault vault)
        {
            _context.Vaults.Add(vault);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Vault vault)
        {
            _context.Vaults.Update(vault);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id, string userId)
        {
            var vault = await GetById(id, userId);
            if (vault != null)
            {
                _context.Vaults.Remove(vault);
                await _context.SaveChangesAsync();
            }
        }
    }
}