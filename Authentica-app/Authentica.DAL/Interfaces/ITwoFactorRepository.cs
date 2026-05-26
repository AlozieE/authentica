namespace Authentica.DAL.Interfaces
{
    public interface ITwoFactorRepository
    {
        Task<(string? secret, string? iv)> GetSecretAsync(string userId);
        Task SaveSecretAsync(string userId, string encryptedSecret, string iv);
        Task ClearSecretAsync(string userId);
    }
}
