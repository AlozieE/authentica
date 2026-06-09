using Authentica.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentica_app.Controllers
{
    [Authorize]
    public class TwoFactorController : Controller
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly UserManager<IdentityUser> _userManager;

        public TwoFactorController(ITwoFactorService twoFactorService, UserManager<IdentityUser> userManager)
        {
            _twoFactorService = twoFactorService;
            _userManager = userManager;
        }

        // Toont het adminpanel voor twofactor authentication met de huidige instellingen van de user.
        public async Task<IActionResult> Manage()
        {
            // Haal de ingelogde user op om te controleren of 2FA al is ingeschakeld.
            var user = await _userManager.GetUserAsync(User);
            ViewBag.TwoFactorEnabled = user!.TwoFactorEnabled;

            if (TempData["SuccessMessage"] is string msg)
                ViewBag.SuccessMessage = msg;
            return View();
        }

        // Toont de QR-code en het geheime sleutel zodat de user een authenticato -app kan koppelen.
        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            var user = await _userManager.GetUserAsync(User);

            // Stuur door naar het adminpaneel als 2FA al actief is, opnieuw instellen is niet nodig.
            if (user!.TwoFactorEnabled)
                return RedirectToAction(nameof(Manage));

            // Genereer een nieuw secret en maak de QR-URI aan die de authenticator-app kan inlezen.
            var secret = _twoFactorService.GenerateSecret();
            ViewBag.Secret = secret;
            ViewBag.QrUri  = _twoFactorService.GetSetupUri(user.Email!, secret);
            return View();
        }

        // Verifieert de ingevoerde code en schakelt 2FA in als de code overeenkomt met de secret.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(string code, string secret)
        {
            var user = await _userManager.GetUserAsync(User);

            // Toon het formulier opnieuw met een foutmelding als de verificatiecode onjuist is.
            if (!_twoFactorService.ValidateCodeWithSecret(secret, code))
            {
                ModelState.AddModelError(nameof(code), "Invalid verification code. Please try again.");
                ViewBag.Secret = secret;
                ViewBag.QrUri  = _twoFactorService.GetSetupUri(user!.Email!, secret);
                return View();
            }

            // Sla de secret op in de database en zet de 2FA flag aan voor de user.
            await _twoFactorService.SaveSecretAsync(user!.Id, secret);
            user.TwoFactorEnabled = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Two-factor authentication is now enabled.";
            return RedirectToAction(nameof(Manage));
        }

        // Schakelt twofactorauthentication uit en verwijdert het opgeslagen secret van de user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable()
        {
            var user = await _userManager.GetUserAsync(User);

            // Zet de 2FA flag uit en verwijder de secret, zodat de user niet langer wordt gevraagd om een code.
            user!.TwoFactorEnabled = false;
            await _userManager.UpdateAsync(user);
            await _twoFactorService.ClearSecretAsync(user.Id);

            TempData["SuccessMessage"] = "Two-factor authentication has been disabled.";
            return RedirectToAction(nameof(Manage));
        }
    }
}
