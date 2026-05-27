using System.ComponentModel.DataAnnotations;

namespace Authentica_app.DTOs
{
    public class VaultCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
    }
}
