using System.ComponentModel.DataAnnotations;
        using Microsoft.AspNetCore.Identity;
        
        namespace Authentica.Models
        {
            public class Vault
            {
                public ICollection<VaultItem> VaultItems { get; set; } = new List<VaultItem>();
                public int VaultId { get; set; }
                
                [Required]
                [StringLength(255)]
                public string Name { get; set; } = string.Empty;
                
                public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                
                public string UserId { get; set;} = string.Empty;
                
                public IdentityUser User { get; set; } = null!;
            }
        }