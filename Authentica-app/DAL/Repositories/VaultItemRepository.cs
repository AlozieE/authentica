using Authentica_app.BLL.Models;
using Authentica_app.DAL.Database;
using Microsoft.Data.SqlClient;

namespace Authentica_app.DAL.Repositories
{
    public class VaultItemRepository
    {
        private readonly DatabaseConnection _db;

        public VaultItemRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public async Task<List<VaultItem>> GetByVault(int vaultId)
        {
            var items = new List<VaultItem>();

            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                SELECT VaultItemId, Title, ItemType, EncryptedData, IV, CreatedAt, UpdatedAt, VaultId
                FROM VaultItems
                WHERE VaultId = @VaultId
                ORDER BY CreatedAt DESC
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VaultId", vaultId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                items.Add(MapVaultItem(reader));

            return items;
        }

        public async Task<List<VaultItem>> GetByUserAndType(string userId, VaultItemType type, int? vaultId)
        {
            var items = new List<VaultItem>();

            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                SELECT vi.VaultItemId, vi.Title, vi.ItemType, vi.EncryptedData, vi.IV,
                       vi.CreatedAt, vi.UpdatedAt, vi.VaultId,
                       v.Name AS VaultName, v.CreatedAt AS VaultCreatedAt, v.UserId AS VaultUserId
                FROM VaultItems vi
                INNER JOIN Vaults v ON vi.VaultId = v.VaultId
                WHERE v.UserId = @UserId
                  AND vi.ItemType = @ItemType
                  AND (@VaultId IS NULL OR vi.VaultId = @VaultId)
                ORDER BY vi.CreatedAt DESC
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ItemType", (int)type);
            cmd.Parameters.AddWithValue("@VaultId", vaultId.HasValue ? vaultId.Value : DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                items.Add(MapVaultItemWithVault(reader));

            return items;
        }

        public async Task<VaultItem?> GetById(int id)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                SELECT vi.VaultItemId, vi.Title, vi.ItemType, vi.EncryptedData, vi.IV,
                       vi.CreatedAt, vi.UpdatedAt, vi.VaultId,
                       v.Name AS VaultName, v.CreatedAt AS VaultCreatedAt, v.UserId AS VaultUserId
                FROM VaultItems vi
                INNER JOIN Vaults v ON vi.VaultId = v.VaultId
                WHERE vi.VaultItemId = @VaultItemId
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VaultItemId", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapVaultItemWithVault(reader) : null;
        }

        public async Task Add(VaultItem item)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                INSERT INTO VaultItems (Title, ItemType, EncryptedData, IV, CreatedAt, UpdatedAt, VaultId)
                OUTPUT INSERTED.VaultItemId
                VALUES (@Title, @ItemType, @EncryptedData, @IV, @CreatedAt, @UpdatedAt, @VaultId)
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@ItemType", (int)item.ItemType);
            cmd.Parameters.AddWithValue("@EncryptedData", item.EncryptedData);
            cmd.Parameters.AddWithValue("@IV", item.IV);
            cmd.Parameters.AddWithValue("@CreatedAt", item.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", item.UpdatedAt);
            cmd.Parameters.AddWithValue("@VaultId", item.VaultId);

            item.VaultItemId = (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task Update(VaultItem item)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = """
                UPDATE VaultItems
                SET Title        = @Title,
                    EncryptedData = @EncryptedData,
                    IV            = @IV,
                    UpdatedAt     = @UpdatedAt
                WHERE VaultItemId = @VaultItemId
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@EncryptedData", item.EncryptedData);
            cmd.Parameters.AddWithValue("@IV", item.IV);
            cmd.Parameters.AddWithValue("@UpdatedAt", item.UpdatedAt);
            cmd.Parameters.AddWithValue("@VaultItemId", item.VaultItemId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(VaultItem item)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            const string sql = "DELETE FROM VaultItems WHERE VaultItemId = @VaultItemId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VaultItemId", item.VaultItemId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static VaultItem MapVaultItem(SqlDataReader reader) => new()
        {
            VaultItemId   = reader.GetInt32(reader.GetOrdinal("VaultItemId")),
            Title         = reader.GetString(reader.GetOrdinal("Title")),
            ItemType      = (VaultItemType)reader.GetInt32(reader.GetOrdinal("ItemType")),
            EncryptedData = reader.GetString(reader.GetOrdinal("EncryptedData")),
            IV            = reader.GetString(reader.GetOrdinal("IV")),
            CreatedAt     = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt     = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            VaultId       = reader.GetInt32(reader.GetOrdinal("VaultId"))
        };

        private static VaultItem MapVaultItemWithVault(SqlDataReader reader)
        {
            var item = MapVaultItem(reader);
            item.Vault = new Vault
            {
                VaultId   = item.VaultId,
                Name      = reader.GetString(reader.GetOrdinal("VaultName")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("VaultCreatedAt")),
                UserId    = reader.GetString(reader.GetOrdinal("VaultUserId"))
            };
            return item;
        }
    }
}
