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

        // UC11 – Show 2FA management overview
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.TwoFactorEnabled = user!.TwoFactorEnabled;
            if (TempData["SuccessMessage"] is string msg)
                ViewBag.SuccessMessage = msg;
            return View();
        }

        // UC11 – Step 1: show secret + QR code
        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user!.TwoFactorEnabled)
                return RedirectToAction(nameof(Manage));

            var secret = _twoFactorService.GenerateSecret();
            ViewBag.Secret = secret;
            ViewBag.QrUri  = _twoFactorService.GetSetupUri(user.Email!, secret);
            return View();
        }

        // UC11 – Step 2: verify code then enable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(string code, string secret)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!_twoFactorService.ValidateCodeWithSecret(secret, code))
            {
                ModelState.AddModelError(nameof(code), "Invalid verification code. Please try again.");
                ViewBag.Secret = secret;
                ViewBag.QrUri  = _twoFactorService.GetSetupUri(user!.Email!, secret);
                return View();
            }

            await _twoFactorService.SaveSecretAsync(user!.Id, secret);
            user.TwoFactorEnabled = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Two-factor authentication is now enabled.";
            return RedirectToAction(nameof(Manage));
        }

        // UC11 – Disable 2FA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable()
        {
            var user = await _userManager.GetUserAsync(User);
            user!.TwoFactorEnabled = false;
            await _userManager.UpdateAsync(user);
            await _twoFactorService.ClearSecretAsync(user.Id);

            TempData["SuccessMessage"] = "Two-factor authentication has been disabled.";
            return RedirectToAction(nameof(Manage));
        }
    }
}
