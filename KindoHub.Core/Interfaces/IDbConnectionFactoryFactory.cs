using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IDbConnectionFactoryFactory
    {
        IDbConnectionFactory Crear(string connectionStringName);
    }
}
