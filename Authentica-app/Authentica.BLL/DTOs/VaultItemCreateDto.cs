using System.ComponentModel.DataAnnotations;

namespace Authentica.BLL.DTOs
{
    public class VaultItemCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        public int VaultItemTypeId { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new();
    }
}
