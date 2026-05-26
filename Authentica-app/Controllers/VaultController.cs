using Authentica.BLL.Interfaces;
using Authentica.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentica_app.Controllers
{
    [Authorize]
    public class VaultController : Controller
    {
        private readonly IVaultService _vaultService;
        private readonly UserManager<IdentityUser> _userManager;

        public VaultController(IVaultService vaultService, UserManager<IdentityUser> userManager)
        {
            _vaultService = vaultService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var vaults = await _vaultService.GetUserVaults(userId);
            return View(vaults);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vault vault)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                vault.UserId = _userManager.GetUserId(User)!;
                vault.CreatedAt = DateTime.UtcNow;
                await _vaultService.CreateVault(vault);
                return RedirectToAction(nameof(Index));
            }
            return View(vault);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(id, userId);

            if (vault == null)
                return NotFound();
            return View(vault);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(id, userId);

            if (vault == null)
                return NotFound();
            return View(vault);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vault vault)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            var userId = _userManager.GetUserId(User)!;
            var existingVault = await _vaultService.GetById(id, userId);

            if (existingVault == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existingVault.Name = vault.Name;
                await _vaultService.UpdateVault(existingVault);
                return RedirectToAction(nameof(Index));
            }
            return View(vault);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(id, userId);

            if (vault == null)
                return NotFound();

            await _vaultService.DeleteVault(vault);
            return RedirectToAction(nameof(Index));
        }
    }
}
