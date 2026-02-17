using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IDbConnectionFactory
    {
        Task<SqlConnection> CreateConnectionAsync();
    }
}
