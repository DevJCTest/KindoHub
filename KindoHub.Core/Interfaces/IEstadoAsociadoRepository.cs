using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IEstadoAsociadoRepository
    {
        Task<IEnumerable<EstadoAsociadoEntity>> LeerTodos();
        Task<EstadoAsociadoEntity?> LeerPredeterminado();
        Task<bool> EstablecerPredeterminado(int id);
        Task<EstadoAsociadoEntity?> LeerPorNombre(string nombre);
        Task<EstadoAsociadoEntity?> LeerPorId(int id);

    }
}
