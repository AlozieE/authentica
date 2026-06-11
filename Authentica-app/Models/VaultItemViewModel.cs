namespace Authentica_app.Models
{
    public class VaultItemViewModel
    {
        public int VaultItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public int VaultId { get; set; }
        public string VaultName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
