namespace Authentica_app.Models
{
    public class VaultViewModel
    {
        public int VaultId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }
}
