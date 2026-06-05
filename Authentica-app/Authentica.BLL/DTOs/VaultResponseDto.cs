namespace Authentica.BLL.DTOs
{
    public class VaultResponseDto
    {
        public int VaultId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
