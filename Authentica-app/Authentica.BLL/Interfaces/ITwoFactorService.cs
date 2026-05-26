namespace Authentica.BLL.Interfaces
{
    public interface ITwoFactorService
    {
        string GenerateSecret();
        string GetSetupUri(string email, string secret);
        bool ValidateCodeWithSecret(string plaintextSecret, string code);
        Task<bool> ValidateCodeAsync(string userId, string code);
        Task SaveSecretAsync(string userId, string plaintextSecret);
        Task ClearSecretAsync(string userId);
    }
}
