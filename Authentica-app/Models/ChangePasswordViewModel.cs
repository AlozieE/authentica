using System.ComponentModel.DataAnnotations;

namespace Authentica_app.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Huidig wachtwoord is verplicht.")]
        [DataType(DataType.Password)]
        [Display(Name = "Huidig wachtwoord")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nieuw wachtwoord is verplicht.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nieuw wachtwoord")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bevestig wachtwoord is verplicht.")]
        [DataType(DataType.Password)]
        [Display(Name = "Bevestig nieuw wachtwoord")]
        [Compare(nameof(NewPassword), ErrorMessage = "De wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
