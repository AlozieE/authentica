using System.ComponentModel.DataAnnotations;
using Authentica.DAL.Models;

namespace Authentica.BLL.DTOs
{
    public class VaultItemCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        public VaultItemType ItemType { get; set; }
    }
}
