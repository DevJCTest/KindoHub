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
        Task<EstadoAsociadoDto?> GetEstadoAsociadoAsync(string name);
        Task<EstadoAsociadoDto?> GetEstadoAsociadoAsync(int id);
        Task<IEnumerable<EstadoAsociadoDto>> GetAllEstadoAsociadoAsync();
    }
}
