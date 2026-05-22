using Authentica_app.BLL.Models;

namespace Authentica_app.Presentation.DTOs
{
    public class VaultItemCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public VaultItemType ItemType { get; set; }
    }
}
