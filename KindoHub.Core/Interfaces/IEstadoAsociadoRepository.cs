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
        Task<EstadoAsociadoEntity?> GetEstadoAsociadoAsync(string nombre);
        Task<EstadoAsociadoEntity?> GetEstadoAsociadoAsync(int id);
        Task<bool> SetPredeterminadoAsync(int id);
        Task<EstadoAsociadoEntity?> GetPredeterminadoAsync();
        Task<IEnumerable<EstadoAsociadoEntity>> GetAllEstadoAsociadoAsync();

    }
}
