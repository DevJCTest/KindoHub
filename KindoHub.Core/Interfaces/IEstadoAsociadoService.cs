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
        Task<(bool Success, string Message, EstadoAsociadoDto? EstadoAsociado)> SetPredeterminadoAsync(int id);
        Task<EstadoAsociadoDto?> GetPredeterminadoAsync();
        Task<IEnumerable<EstadoAsociadoDto>> GetAllEstadoAsociadoAsync();
    }
}
