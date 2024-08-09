using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace MotoFacts.Context
{
    public class DapperContext : IDapperContext
    {
        private readonly IConfiguration _config;

        public DapperContext(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection GetConnection()
        {
            return new SqliteConnection(_config.GetConnectionString("DefaultConnection"));
        }
    }
}
