using Authentica.BLL.Interfaces;
using Authentica.DAL.Models;
using Authentica.BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentica_app.Controllers
{
    [Authorize]
    public class VaultItemController : Controller
    {
        private readonly IVaultItemService _vaultItemService;
        private readonly IVaultService _vaultService;
        private readonly UserManager<IdentityUser> _userManager;

        public VaultItemController(
            IVaultItemService vaultItemService,
            IVaultService vaultService,
            UserManager<IdentityUser> userManager)
        {
            _vaultItemService = vaultItemService;
            _vaultService = vaultService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(VaultItemType type, int? vaultId = null)
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _vaultItemService.GetItems(userId, type, vaultId);

            ViewBag.ItemType = type;
            ViewBag.VaultId = vaultId;
            return View(items);
        }

        public async Task<IActionResult> Create(int vaultId, VaultItemType type)
        {
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(vaultId, userId);

            if (vault == null)
                return NotFound();

            ViewBag.Vault = vault;
            ViewBag.ItemType = type;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int vaultId, VaultItemType type, VaultItemCreateDto dto, IFormCollection form)
        {
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(vaultId, userId);

            if (vault == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Vault = vault;
                ViewBag.ItemType = type;
                return View();
            }

            var data = new Dictionary<string, string>();
            foreach (var key in form.Keys)
            {
                if (key != "__RequestVerificationToken")
                    data[key] = form[key]!;
            }

            var item = new VaultItem
            {
                Title = dto.Title,
                ItemType = type,
                VaultId = vaultId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _vaultItemService.CreateItem(item, data);
            return RedirectToAction(nameof(Index), new { type });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            ViewBag.Data = _vaultItemService.DecryptItemData(item);
            ViewBag.ItemType = item.ItemType;
            ViewBag.Vault = item.Vault;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VaultItemCreateDto dto, IFormCollection form)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Data = _vaultItemService.DecryptItemData(item);
                ViewBag.ItemType = item.ItemType;
                ViewBag.Vault = item.Vault;
                return View(item);
            }

            var data = new Dictionary<string, string>();
            foreach (var key in form.Keys)
            {
                if (key != "__RequestVerificationToken")
                    data[key] = form[key]!;
            }

            item.Title = dto.Title;
            await _vaultItemService.UpdateItem(item, data);

            return RedirectToAction(nameof(Index), new { type = item.ItemType });
        }

        [HttpGet]
        public async Task<IActionResult> GetDecryptedValue(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            var data = _vaultItemService.DecryptItemData(item);

            var value = item.ItemType switch
            {
                VaultItemType.Password   => data.GetValueOrDefault("Password"),
                VaultItemType.CreditCard => data.GetValueOrDefault("CardNumber"),
                VaultItemType.SecureNote => data.GetValueOrDefault("Content"),
                _                        => null
            };

            return Json(new { value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            var type = item.ItemType;
            await _vaultItemService.DeleteItem(item);

            return RedirectToAction(nameof(Index), new { type });
        }
    }
}
