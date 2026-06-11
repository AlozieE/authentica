using System.ComponentModel.DataAnnotations;
using Authentica.BLL.Models;

namespace Authentica.BLL.DTOs
{
    public class VaultItemCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        public VaultItemType ItemType { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new();
    }
}
