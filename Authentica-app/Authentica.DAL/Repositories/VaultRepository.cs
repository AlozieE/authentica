using Authentica.DAL.Models;
using Authentica.DAL.Database;
using Authentica.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace Authentica.DAL.Repositories
{
    public class VaultRepository : IVaultRepository
    {
        private readonly DatabaseConnection _db;

        public VaultRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public async Task<List<Vault>> GetAll(string userId)
        {
            var vaults = new List<Vault>();

            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                SELECT VaultId, Name, CreatedAt, UserId
                FROM Vaults
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                vaults.Add(MapVault(reader));

            return vaults;
        }

        public async Task<Vault?> GetById(int id, string userId)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                SELECT VaultId, Name, CreatedAt, UserId
                FROM Vaults
                WHERE VaultId = @VaultId AND UserId = @UserId
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VaultId", id);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapVault(reader) : null;
        }

        public async Task Add(Vault vault)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                INSERT INTO Vaults (Name, CreatedAt, UserId)
                OUTPUT INSERTED.VaultId
                VALUES (@Name, @CreatedAt, @UserId)
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", vault.Name);
            cmd.Parameters.AddWithValue("@CreatedAt", vault.CreatedAt);
            cmd.Parameters.AddWithValue("@UserId", vault.UserId);

            vault.VaultId = (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task Update(Vault vault)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                UPDATE Vaults
                SET Name = @Name
                WHERE VaultId = @VaultId AND UserId = @UserId
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", vault.Name);
            cmd.Parameters.AddWithValue("@VaultId", vault.VaultId);
            cmd.Parameters.AddWithValue("@UserId", vault.UserId);

            await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected == 0 )
             throw new Exception("Vault niet gevonden of geen toegang");
        }

        public async Task Delete(Vault vault)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = "DELETE FROM Vaults WHERE VaultId = @VaultId AND UserId = @UserId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VaultId", vault.VaultId);
            cmd.Parameters.AddWithValue("@UserId", vault.UserId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected == 0)
                throw new Exception("Vault niet gevonden of geen toegang");
        }

        private static Vault MapVault(SqlDataReader reader) => new()
        {
            VaultId   = reader.GetInt32(reader.GetOrdinal("VaultId")),
            Name      = reader.GetString(reader.GetOrdinal("Name")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UserId    = reader.GetString(reader.GetOrdinal("UserId"))
        };
    }
}
