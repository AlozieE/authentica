using System.ComponentModel.DataAnnotations;

namespace Authentica.BLL.Models
{
    public class VaultItem
    {
        public int VaultItemId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int VaultItemTypeId { get; set; }

        [Required]
        public string EncryptedData { get; set; } = string.Empty;

        [Required]
        public string IV { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int VaultId { get; set; }
        public Vault Vault { get; set; } = null!;
    }
}
