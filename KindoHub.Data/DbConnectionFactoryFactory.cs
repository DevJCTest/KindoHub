using KindoHub.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data
{
    public class DbConnectionFactoryFactory : IDbConnectionFactoryFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactoryFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnectionFactory Crear(string connectionStringName)
        {
            return new SqlConnectionFactory(_configuration, connectionStringName);
        }
    }
}
