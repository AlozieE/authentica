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

        public async Task<IActionResult> Index(VaultItemType type, int? vaultId = null)
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.VaultItems
                .Include(i => i.Vault)
                .Where(i => i.Vault.UserId == userId && i.ItemType == type);

            if (vaultId.HasValue)
                query = query.Where(i => i.VaultId == vaultId.Value);

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            ViewBag.ItemType = type;
            ViewBag.VaultId = vaultId;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int vaultId, VaultItemType type, IFormCollection form)
        {
            var userId = _userManager.GetUserId(User);

            var vault = await _context.Vaults
                .FirstOrDefaultAsync(v => v.VaultId == vaultId && v.UserId == userId);

            if (vault == null)
                return NotFound();

            var data = new Dictionary<string, string>();
            foreach (var key in form.Keys)
            {
                if (key != "__RequestVerificationToken")
                    data[key] = form[key]!;
            }

            var title = form["Title"].ToString();

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("Title", "Titel is verplicht.");
                ViewBag.Vault = vault;
                ViewBag.ItemType = type;
                return View();
            }

            var item = new VaultItem
            {
                Title = title,
                ItemType = type,
                VaultId = vaultId,
                EncryptedData = JsonSerializer.Serialize(data),
                IV = "placeholder",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.VaultItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _context.VaultItems
                .Include(i => i.Vault)
                .FirstOrDefaultAsync(i => i.VaultItemId == id && i.Vault.UserId == userId);

            if (item == null)
                return NotFound();

            ViewBag.Data = JsonSerializer.Deserialize<Dictionary<string, string>>(item.EncryptedData);
            ViewBag.ItemType = item.ItemType;
            ViewBag.Vault = item.Vault;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _context.VaultItems
                .Include(i => i.Vault)
                .FirstOrDefaultAsync(i => i.VaultItemId == id && i.Vault.UserId == userId);

            if (item == null)
                return NotFound();

            var data = new Dictionary<string, string>();
            foreach (var key in form.Keys)
            {
                if (key != "__RequestVerificationToken")
                    data[key] = form[key]!;
            }

            item.Title = form["Title"].ToString();
            item.EncryptedData = JsonSerializer.Serialize(data);
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type = item.ItemType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _context.VaultItems
                .Include(i => i.Vault)
                .FirstOrDefaultAsync(i => i.VaultItemId == id && i.Vault.UserId == userId);

            if (item == null)
                return NotFound();

            var type = item.ItemType;
            _context.VaultItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type });
        }
    }
}