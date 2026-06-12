using System.ComponentModel.DataAnnotations;

namespace Authentica_app.Models
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Nieuw e-mailadres is verplicht.")]
        [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
        [Display(Name = "Nieuw e-mailadres")]
        public string NewEmail { get; set; } = string.Empty;
    }
}
