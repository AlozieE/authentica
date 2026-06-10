using Authentica.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Authentica.BLL.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        // Laadt de AES-256 key uit appsettings.json en convert deze van Base64 naar bytes.
        public EncryptionService(IConfiguration configuration)
        {
            var keyBase64 = configuration["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption:Key is not configured in appsettings.json.");
            _key = Convert.FromBase64String(keyBase64);
        }

        // Versleutelt de tekst met AES en een willekeurige IV per versleuteling.
        // Geeft zowel de versleutelde data als de IV terug zodat ze apart opgeslagen kunnen worden.
        public (string encryptedData, string iv) Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return (Convert.ToBase64String(encryptedBytes), Convert.ToBase64String(aes.IV));
        }

        // Ontsleutelt de data met de opgegeven IV en de gedeelde AES key.
        public string Decrypt(string encryptedData, string iv)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = Convert.FromBase64String(iv);

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        // Genereert een willekeurige IV zonder te versleutelen, voor los gebruik indien nodig.
        public string GenerateIV()
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            return Convert.ToBase64String(aes.IV);
        }
    }
}