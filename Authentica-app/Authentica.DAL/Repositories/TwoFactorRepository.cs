using Authentica.DAL.Database;
using Authentica.DAL.Interfaces;
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

        public async Task SaveSecretAsync(string userId, string encryptedSecret, string iv)
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
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ClearSecretAsync(string userId)
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
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
