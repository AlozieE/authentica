using Authentica.BLL.Interfaces;
using Authentica.DAL.Interfaces;
using OtpNet;

namespace Authentica.BLL.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ITwoFactorRepository _repository;
        private readonly EncryptionService _encryption;
        private const string Issuer = "Authentica";

        public TwoFactorService(ITwoFactorRepository repository, EncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public string GenerateSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public string GetSetupUri(string email, string secret)
        {
            var encodedIssuer = Uri.EscapeDataString(Issuer);
            var encodedEmail  = Uri.EscapeDataString(email);
            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
        }

        public bool ValidateCodeWithSecret(string plaintextSecret, string code)
        {
            var secretBytes = Base32Encoding.ToBytes(plaintextSecret);
            var totp = new Totp(secretBytes);
            return totp.VerifyTotp(code.Replace(" ", "").Replace("-", ""), out _, new VerificationWindow(1, 1));
        }

        public async Task<bool> ValidateCodeAsync(string userId, string code)
        {
            var (encSecret, iv) = await _repository.GetSecretAsync(userId);
            if (encSecret is null || iv is null)
                return false;

            var plaintextSecret = _encryption.Decrypt(encSecret, iv);
            return ValidateCodeWithSecret(plaintextSecret, code);
        }

        public async Task SaveSecretAsync(string userId, string plaintextSecret)
        {
            var (encrypted, iv) = _encryption.Encrypt(plaintextSecret);
            await _repository.SaveSecretAsync(userId, encrypted, iv);
        }

        public async Task ClearSecretAsync(string userId) =>
            await _repository.ClearSecretAsync(userId);
    }
}
