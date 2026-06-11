using Authentica.DAL.Database;
using Authentica.BLL.Interfaces;
using Microsoft.Data.SqlClient;

namespace Authentica.DAL.Repositories
{
    public class TwoFactorRepository : ITwoFactorRepository
    {
        private readonly DatabaseConnection _db;

        public TwoFactorRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public async Task<(string? secret, string? iv)> GetSecretAsync(string userId)
        {
            try
            {
                await using var conn = _db.CreateConnection();
                await conn.OpenAsync();

                const string sql = """
                    SELECT TwoFactorSecret, TwoFactorSecretIV
                    FROM AspNetUsers
                    WHERE Id = @UserId
                    """;

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return (null, null);

                var secretOrd = reader.GetOrdinal("TwoFactorSecret");
                var ivOrd     = reader.GetOrdinal("TwoFactorSecretIV");

                return (
                    reader.IsDBNull(secretOrd) ? null : reader.GetString(secretOrd),
                    reader.IsDBNull(ivOrd)     ? null : reader.GetString(ivOrd)
                );
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve 2FA secret for user '{userId}'.", ex);
            }
        }

        public async Task SaveSecretAsync(string userId, string encryptedSecret, string iv)
        {
            try
            {
                await using var conn = _db.CreateConnection();
                await conn.OpenAsync();

                const string sql = """
                    UPDATE AspNetUsers
                    SET TwoFactorSecret   = @Secret,
                        TwoFactorSecretIV = @IV
                    WHERE Id = @UserId
                    """;

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Secret", encryptedSecret);
                cmd.Parameters.AddWithValue("@IV",     iv);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    throw new InvalidOperationException($"User '{userId}' not found; 2FA secret was not saved.");
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Failed to save 2FA secret for user '{userId}'.", ex);
            }
        }

        public async Task ClearSecretAsync(string userId)
        {
            try
            {
                await using var conn = _db.CreateConnection();
                await conn.OpenAsync();

                const string sql = """
                    UPDATE AspNetUsers
                    SET TwoFactorSecret   = NULL,
                        TwoFactorSecretIV = NULL
                    WHERE Id = @UserId
                    """;

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    throw new InvalidOperationException($"User '{userId}' not found; 2FA secret was not cleared.");
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Failed to clear 2FA secret for user '{userId}'.", ex);
            }
        }
    }
}
