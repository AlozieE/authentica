using Authentica_app.BLL.Models;
using Authentica_app.DAL.Database;
using Microsoft.EntityFrameworkCore;

namespace Authentica_app.DAL.Repositories
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

        public async Task Update()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Vault vault)
        {
            _context.Vaults.Remove(vault);
            await _context.SaveChangesAsync();
        }
    }
}
