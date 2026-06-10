using Authentica.BLL.Interfaces;
using Authentica.DAL.Models;
using Authentica.BLL.DTOs;
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

        // Toont een overzicht van alle vaults die toebehoren aan de ingelogde user.
        public async Task<IActionResult> Index()
        {
            // Haal alleen vaults op die gekoppeld zijn aan de huidige user.
            var userId = _userManager.GetUserId(User)!;
            var vaults = await _vaultService.GetUserVaults(userId);
            return View(vaults);
        }

        // Toont het formulier voor het aanmaken van een nieuwe vault.
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Verwerkt het ingediende formulier en slaat de nieuwe vault op voor de ingelogde user.
        public async Task<IActionResult> Create(VaultCreateDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Koppel de nieuwe vault aan de ingelogde user via zijn userId.
                    var userId = _userManager.GetUserId(User)!;
                    await _vaultService.CreateVault(userId, dto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    // Toon een algemene foutmelding als het aanmaken mislukt door een onverwachte fout.
                    ModelState.AddModelError("", "Er is iets misgegaan bij het aanmaken van de vault.");
                }
            }
            return View(dto);
        }

        // Toont de detailpagina van een specifieke vault, inclusief de bijbehorende items.
        public async Task<IActionResult> Details(int id)
        {
            // Haal de vault op en controleer meteen of hij toebehoort aan de ingelogde user.
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(id, userId);

            // Geef 404 error terug als de vault niet bestaat of bij een andere user hoort.
            if (vault == null)
                return NotFound();
            return View(vault);
        }

        // Toont het bewerkingsformulier voor een bestaande vault, gevuld met de huidige naam.
        public async Task<IActionResult> Edit(int id)
        {
            // Controleer of de vault bestaat en hoort bij de huidige user voordat het formulier wordt getoond.
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(id, userId);

            // Geef 404 error terug als de vault niet bestaat of bij een andere user hoort.
            if (vault == null)
                return NotFound();

            // Vul het DTO vooraf in met de huidige naam zodat het formulier bewerkbaar is.
            return View(new VaultCreateDto { Name = vault.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Verwerkt het ingediende bewerkingsformulier en slaat de bijgewerkte vaultnaam op.
        public async Task<IActionResult> Edit(int id, VaultCreateDto dto)
        {
            // Controleer opnieuw of de vault bestaat en hoort bij de huidige user.
            var userId = _userManager.GetUserId(User)!;
            var existingVault = await _vaultService.GetById(id, userId);

            // Geef 404 error terug als de vault niet gevonden wordt.
            if (existingVault == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Werk alleen de naam bij; andere vaulteigenschappen blijven ongewijzigd.
                    existingVault.Name = dto.Name;
                    await _vaultService.UpdateVault(existingVault);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    // Toon een foutmelding als het bijwerken mislukt door een onverwachte fout.
                    ModelState.AddModelError("", "Er is iets misgegaan bij het bijwerken van de vault.");
                }
            }
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Verwijdert een vault na verificatie dat de vault aan de ingelogde user toebehoort.
        public async Task<IActionResult> Delete(int id)
        {
            // Haal de vault op en controleer ownership om te voorkomen dat iemand andermans vaults verwijdert.
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(id, userId);

            // Geef 404 error terug als de vault niet bestaat of bij een andere user hoort.
            if (vault == null)
                return NotFound();

            try
            {
                await _vaultService.DeleteVault(vault);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                // Bij een fout sturen we de user terug naar het overzicht zonder foutmelding,
                // omdat er geen formulier is om een ModelState-fout op te tonen.
                return RedirectToAction(nameof(Index));
            }
        }
    }
}