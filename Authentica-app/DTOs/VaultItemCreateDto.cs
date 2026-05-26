using Authentica.DAL.Models;

namespace Authentica_app.DTOs
{
    public class VaultItemCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public VaultItemType ItemType { get; set; }
    }
}
