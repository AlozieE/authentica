using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace Authentica.DAL.Database
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
