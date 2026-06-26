using Authentica.BLL.Interfaces;
using OtpNet;

namespace Authentica.BLL.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ITwoFactorRepository _repository;
        private readonly IEncryptionService _encryption;
        private const string Issuer = "Authentica";

        public TwoFactorService(ITwoFactorRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        // Genereert een willekeurige Base32 key voor gebruik in een authenticator-app.
        public string GenerateSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        // Bouwt een otpauth-URI op die als QR-code kan worden ingelezen door een authenticator-app.
        public string GetSetupUri(string email, string secret)
        {
            var encodedIssuer = Uri.EscapeDataString(Issuer);
            var encodedEmail  = Uri.EscapeDataString(email);
            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
        }

        // Valideert de code tegen het plaintext secret met een tijdvenster van 30 seconden.
        public bool ValidateCodeWithSecret(string plaintextSecret, string code)
        {
            var secretBytes = Base32Encoding.ToBytes(plaintextSecret);
            var totp = new Totp(secretBytes);
            var result = totp.VerifyTotp(code.Replace(" ", "").Replace("-", ""), out _, new VerificationWindow(1, 1));
            return result;
        }

        // Haalt het versleutelde secret op, ontsleutelt het en valideert de ingevoerde code.
        public async Task<bool> ValidateCodeAsync(string userId, string code)
        { 
            var (encSecret, iv) = await _repository.GetSecretAsync(userId);
            if (encSecret is null || iv is null)
                return false;

            var plaintextSecret = _encryption.Decrypt(encSecret, iv);
            return ValidateCodeWithSecret(plaintextSecret, code);
        }

        // Versleutelt de secret met AES voordat het wordt opgeslagen in de database.
        public async Task SaveSecretAsync(string userId, string plaintextSecret)
        {
            var (encrypted, iv) = _encryption.Encrypt(plaintextSecret);
            await _repository.SaveSecretAsync(userId, encrypted, iv);
        }

        // Verwijdert het opgeslagen secret wanneer 2FA wordt uitgeschakeld.
        public async Task ClearSecretAsync(string userId) =>
            await _repository.ClearSecretAsync(userId);
    }
}