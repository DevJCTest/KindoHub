using KindoHub.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public SqlConnectionFactory(IConfiguration configuration, string connectionStringName = "DefaultConnection")
        {
            _connectionString = configuration.GetConnectionString(connectionStringName)
           ?? throw new InvalidOperationException(
               $"No se encontró la cadena de conexión '{connectionStringName}' en la configuración. " +
               "Asegúrese de que existe el archivo appsettings.json con la configuración correcta.");
        }
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public Task<SqlConnection> CreateConnectionAsync()
        {
            return Task.FromResult(CreateConnection());
        }
    }
}
