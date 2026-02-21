using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface ILogService
    {
        Task<IEnumerable<LogDto>> LeerTodos();
        Task<IEnumerable<LogDto>> LeerFiltrado(FilterOptions[] filters);
        IEnumerable<LogFieldDto> ObtenerCamposDisponibles();
    }
}
