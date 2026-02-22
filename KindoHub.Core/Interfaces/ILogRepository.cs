using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface ILogRepository
    {
        Task<IEnumerable<LogEntity>> LeerTodos();
        Task<IEnumerable<LogEntity>> LeerFiltrado(FilterLogOptions[] filters);
    }
}
