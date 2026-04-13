using Authentica_app.Data;
using Authentica_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Authentica_app.Controllers
{
    [Authorize]
    public class VaultItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public VaultItemController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        public async Task<IActionResult> Index(int vaultId, VaultItemType type)
        {
            var userId = _userManager.GetUserId(User);
            var vault = await _context.Vaults
                .FirstOrDefaultAsync(v => v.VaultId == vaultId && v.UserId == userId);
                
            if (vault == null)
            return NotFound();
            
            var items = await _context.VaultItems
                .Where(i => i.VaultId == vaultId && i.ItemType == type)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
                
            ViewBag.Vault = vault;
            ViewBag.ItemType = type;
            
            return View(items);
        }
        
        public async Task<IActionResult> Create(int vaultId, VaultItemType type)
        {
          var userId = _userManager.GetUserId(User);
          var vault = await _context.Vaults
                .FirstOrDefaultAsync(v => v.VaultId == vaultId && v.UserId == userId);
                
            if (vault == null)
                return NotFound();
                
                ViewBag.Vault = vault;
                ViewBag.ItemType = type;
                
            return View();
        }
    }
}