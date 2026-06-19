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
            try
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
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Fout bij het ophalen van vault item types.", ex);
            }
        }

        public VaultItemType GetById(int id)
        {
            try
            {
                using var conn = _db.CreateConnection();
                conn.Open();

                const string sql = "SELECT VaultItemTypeId, Name FROM VaultItemType WHERE VaultItemTypeId = @Id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    throw new InvalidOperationException($"VaultItemType met id {id} niet gevonden.");

                return MapVaultItemType(reader);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Fout bij het ophalen van vault item type met id {id}.", ex);
            }
        }

        private static VaultItemType MapVaultItemType(SqlDataReader reader) => new()
        {
            VaultItemTypeId = reader.GetInt32(reader.GetOrdinal("VaultItemTypeId")),
            Name            = reader.GetString(reader.GetOrdinal("Name"))
        };
    }
}