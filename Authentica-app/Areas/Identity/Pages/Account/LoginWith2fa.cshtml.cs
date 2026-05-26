using System.ComponentModel.DataAnnotations;
using Authentica.BLL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Authentica_app.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITwoFactorService _twoFactorService;

        public LoginWith2faModel(SignInManager<IdentityUser> signInManager, ITwoFactorService twoFactorService)
        {
            _signInManager = signInManager;
            _twoFactorService = twoFactorService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your authenticator code.")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be exactly 6 digits.")]
            [DataType(DataType.Text)]
            public string TwoFactorCode { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user is null)
                return RedirectToPage("./Login");

            RememberMe = rememberMe;
            ReturnUrl  = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
        {
            ReturnUrl  = returnUrl ?? Url.Content("~/");
            RememberMe = rememberMe;

            if (!ModelState.IsValid)
                return Page();

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user is null)
                return RedirectToPage("./Login");

            var code = Input.TwoFactorCode.Replace(" ", "").Replace("-", "");
            var valid = await _twoFactorService.ValidateCodeAsync(user.Id, code);

            if (!valid)
            {
                ModelState.AddModelError(string.Empty, "Invalid authenticator code. Please try again.");
                return Page();
            }

            await _signInManager.SignInAsync(user, rememberMe);
            return LocalRedirect(ReturnUrl);
        }
    }
}
