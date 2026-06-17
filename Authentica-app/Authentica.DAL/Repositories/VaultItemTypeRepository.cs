using Authentica.BLL.Interfaces;
using Authentica.BLL.Models;
using Authentica.DAL.Database;
using Microsoft.Data.SqlClient;

namespace Authentica.DAL.Repositories
{
    public class VaultItemTypeRepository : IVaultItemTypeRepository
    {
        private readonly DatabaseConnection _db;

        public VaultItemTypeRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public IEnumerable<VaultItemType> GetAll()
        {
            var types = new List<VaultItemType>();

            using var conn = _db.CreateConnection();
            conn.Open();

            const string sql = "SELECT VaultItemTypeId, Name FROM VaultItemType";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                types.Add(MapVaultItemType(reader));

            return types;
        }

        public VaultItemType GetById(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            const string sql = "SELECT VaultItemTypeId, Name FROM VaultItemType WHERE VaultItemTypeId = @Id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                throw new Exception($"VaultItemType with id {id} not found.");

            return MapVaultItemType(reader);
        }

        private static VaultItemType MapVaultItemType(SqlDataReader reader) => new()
        {
            VaultItemTypeId = reader.GetInt32(reader.GetOrdinal("VaultItemTypeId")),
            Name            = reader.GetString(reader.GetOrdinal("Name"))
        };
    }
}
