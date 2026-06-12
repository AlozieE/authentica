using Authentica.BLL.Interfaces;
using Authentica_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentica_app.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            IAccountService accountService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _accountService = accountService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (TempData["SuccessMessage"] is string msg)
                ViewBag.SuccessMessage = msg;

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            var result = await _accountService.ChangePasswordAsync(user!, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateError(error));
                return View(model);
            }

            // Vernieuw de auth-cookie zodat de gebruiker ingelogd blijft na de wachtwoordwijziging.
            await _signInManager.RefreshSignInAsync(user!);
            TempData["SuccessMessage"] = "Wachtwoord is succesvol gewijzigd.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.CurrentEmail = user!.Email;

            if (TempData["SuccessMessage"] is string msg)
                ViewBag.SuccessMessage = msg;

            return View(new ChangeEmailViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.CurrentEmail = user!.Email;

            if (!ModelState.IsValid)
                return View(model);

            if (model.NewEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Het nieuwe e-mailadres is hetzelfde als het huidige.");
                return View(model);
            }

            var result = await _accountService.ChangeEmailAsync(user, model.NewEmail);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateError(error));
                return View(model);
            }

            // Vernieuw de auth-cookie zodat de UI het nieuwe e-mailadres toont.
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "E-mailadres is succesvol gewijzigd.";
            return RedirectToAction(nameof(ChangeEmail));
        }

        private static string TranslateError(IdentityError error) => error.Code switch
        {
            "PasswordMismatch"                => "Huidig wachtwoord is onjuist.",
            "PasswordTooShort"                => "Nieuw wachtwoord is te kort.",
            "PasswordRequiresUpper"           => "Wachtwoord moet minimaal één hoofdletter bevatten.",
            "PasswordRequiresLower"           => "Wachtwoord moet minimaal één kleine letter bevatten.",
            "PasswordRequiresDigit"           => "Wachtwoord moet minimaal één cijfer bevatten.",
            "PasswordRequiresNonAlphanumeric" => "Wachtwoord moet minimaal één speciaal teken bevatten.",
            "DuplicateEmail"                  => "Dit e-mailadres is al in gebruik.",
            "InvalidEmail"                    => "Ongeldig e-mailadres.",
            _                                 => "Er is iets misgegaan. Probeer het opnieuw."
        };
    }
}
