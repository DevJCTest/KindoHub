using KindoHub.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IEstadoAsociadoService
    {
        Task<IEnumerable<EstadoAsociadoDto>> LeerTodos();
        Task<EstadoAsociadoDto?> LeerPredeterminado();
        Task<(bool Success, EstadoAsociadoDto? EstadoAsociado)> EstablecerPredeterminado(int id);
        Task<EstadoAsociadoDto?> LeerPorNombre(string name);

        
    }
}
