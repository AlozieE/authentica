using Authentica.BLL.Interfaces;
using Authentica.BLL.Models;
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
        private readonly IPasswordGeneratorService _passwordGenerator;

        public VaultItemController(
            IVaultItemService vaultItemService,
            IVaultService vaultService,
            UserManager<IdentityUser> userManager,
            IPasswordGeneratorService passwordGenerator)
        {
            _vaultItemService = vaultItemService;
            _vaultService = vaultService;
            _userManager = userManager;
            _passwordGenerator = passwordGenerator;
        }

        // Toont een overzicht van alle vault items van het opgegeven type.
        public async Task<IActionResult> Index(VaultItemType type, int? vaultId = null)
        {
            // Haal alleen items op die aan de ingelogde user horen.
            var userId = _userManager.GetUserId(User)!;

            // Als vaultId is meegegeven worden alleen items uit die specifieke vault getoond.
            var items = await _vaultItemService.GetItems(userId, type, vaultId);

            // Geef het type en de vault id mee aan de view voor filtering en weergave.
            ViewBag.ItemType = type;
            ViewBag.VaultId = vaultId;
            return View(items);
        }

        // Toont het formulier voor het aanmaken van een nieuw vault item van het opgegeven type.
        public async Task<IActionResult> Create(int vaultId, VaultItemType type)
        {
            // Haal de ingelogde user op om eigenaarschap van de vault te controleren.
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(vaultId, userId);

            // Geef 404 error terug als de vault niet bestaat of niet van deze user is.
            if (vault == null)
                return NotFound();

            // Geef de vault en het itemtype mee aan de view zodat het formulier correct wordt opgebouwd.
            ViewBag.Vault = vault;
            ViewBag.ItemType = type;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Verwerkt het ingediende formulier en slaat het nieuwe vault item versleuteld op.
        public async Task<IActionResult> Create(int vaultId, VaultItemType type, VaultItemCreateDto dto, IFormCollection form)
        {
            // Controleer opnieuw of de vault bestaat en hoort bij de huidige user.
            var userId = _userManager.GetUserId(User)!;
            var vault = await _vaultService.GetById(vaultId, userId);

            if (vault == null)
                return NotFound();

            // Toon het formulier opnieuw bij validatiefouten (bijv. verplichte velden ontbreken).
            if (!ModelState.IsValid)
            {
                ViewBag.Vault = vault;
                ViewBag.ItemType = type;
                return View();
            }

            try
            {
                dto.ItemType = type;

                // Verwerk alle formuliervelden als dynamische gegevens, behalve het CSRF token.
                foreach (var key in form.Keys)
                {
                    if (key != "__RequestVerificationToken")
                        dto.Fields[key] = form[key]!;
                }

                // Sla het item op via de service, die de veldwaarden encrypt voor opslag.
                await _vaultItemService.CreateItem(vaultId, userId, dto);
                return RedirectToAction(nameof(Index), new { type });
            }
            catch (Exception)
            {
                // Toon een foutmelding als het aanmaken mislukt door een onverwachte fout.
                ModelState.AddModelError("", "Er is iets misgegaan bij het aanmaken van het item.");
                ViewBag.Vault = vault;
                ViewBag.ItemType = type;
                return View();
            }
        }

        // Toont het bewerkingsformulier voor een bestaand vault item met de ontsleutelde huidige waarden.
        public async Task<IActionResult> Edit(int id)
        {
            // Haal de ingelogde user op om te controleren of het item bij hem hoort.
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            // Geef 404 error terug als het item niet bestaat of bij een andere user hoort.
            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            // decrypt de veldwaarden zodat het formulier leesbaar wordt.
            ViewBag.Data = _vaultItemService.DecryptItemData(item);
            ViewBag.ItemType = item.ItemType;
            ViewBag.Vault = item.Vault;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Verwerkt het ingediende bewerkingsformulier en slaat de bijgewerkte gegevens versleuteld op.
        public async Task<IActionResult> Edit(int id, VaultItemCreateDto dto, IFormCollection form)
        {
            // Controleer opnieuw of het item bestaat en hoort bij de huidige user.
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            // Toon het formulier opnieuw bij validatiefouten, met de huidige ontsleutelde waarden.
            if (!ModelState.IsValid)
            {
                ViewBag.Data = _vaultItemService.DecryptItemData(item);
                ViewBag.ItemType = item.ItemType;
                ViewBag.Vault = item.Vault;
                return View(item);
            }

            try
            {
                // Verzamel alle formuliervelden als de nieuwe veldwaarden, exclusief het CSRF-token.
                var data = new Dictionary<string, string>();
                foreach (var key in form.Keys)
                {
                    if (key != "__RequestVerificationToken")
                        data[key] = form[key]!;
                }

                // Werk de titel bij via het DTO en sla alle veldwaarden versleuteld op via de service.
                item.Title = dto.Title;
                await _vaultItemService.UpdateItem(item, data);
                return RedirectToAction(nameof(Index), new { type = item.ItemType });
            }
            catch (Exception)
            {
                // Toon een foutmelding als het bijwerken mislukt door een onverwachte fout.
                ModelState.AddModelError("", "Er is iets misgegaan bij het bijwerken van het item.");
                ViewBag.Data = _vaultItemService.DecryptItemData(item);
                ViewBag.ItemType = item.ItemType;
                ViewBag.Vault = item.Vault;
                return View(item);
            }
        }

        // Geeft de ontsleutelde waarde van een vault item terug als JSON, bedoeld voor gebruik via JavaScript bijoorbeeld kopiëren naar klembord.
        [HttpGet]
        public async Task<IActionResult> GetDecryptedValue(int id)
        {
            // Controleer of het item bestaat en toebehoort aan de ingelogde user.
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            // Ontsleutel alle velden van het vault item.
            var data = _vaultItemService.DecryptItemData(item);

            // Selecteer het relevante veld op basis van het itemtype (wachtwoord, creditcard of notitie).
            var value = item.ItemType switch
            {
                VaultItemType.Password   => data.GetValueOrDefault("Password"),
                VaultItemType.CreditCard => data.GetValueOrDefault("CardNumber"),
                VaultItemType.SecureNote => data.GetValueOrDefault("Content"),
                _                        => null
            };

            return Json(new { value });
        }

        [HttpGet]
        public IActionResult GeneratePassword(int length = 16)
        {
            if (length < 8 || length > 128)
                return BadRequest(new { error = "Length must be between 8 and 128." });

            var password = _passwordGenerator.Generate(length);
            return Json(new { password });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Verwijdert een vault item na verificatie dat het item aan de ingelogde user toebehoort.
        public async Task<IActionResult> Delete(int id)
        {
            // Haal de ingelogde user op om te voorkomen dat iemand andermans items kan verwijderen.
            var userId = _userManager.GetUserId(User)!;
            var item = await _vaultItemService.GetById(id);

            // Geef 404 error terug als het item niet bestaat of bij een andere user hoort.
            if (item == null || item.Vault.UserId != userId)
                return NotFound();

            try
            {
                // Sla het itemtype op vóór verwijdering, zodat we daarna correct kunnen omleiden.
                var type = item.ItemType;
                await _vaultItemService.DeleteItem(item);
                return RedirectToAction(nameof(Index), new { type });
            }
            catch (Exception)
            {
                // Bij een fout sturen we de user terug naar de overzichtspagina zonder foutmelding,
                // omdat er geen formulier is om een ModelState-fout op te tonen.
                return RedirectToAction(nameof(Index), new { type = item.ItemType });
            }
        }
    }
}