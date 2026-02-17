using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface ICursoRepository
    {
        Task<CursoEntity?> GetByIdAsync(int cursoId);
        Task<IEnumerable<CursoEntity>> GetAllAsync();
        Task<CursoEntity?> GetPredeterminadoAsync();
        Task<CursoEntity?> CreateAsync(CursoEntity curso, string usuarioActual);
        Task<bool> UpdateAsync(CursoEntity curso, string usuarioActual);
        Task<bool> DeleteAsync(int cursoId, byte[] versionFila);
        Task<bool> SetPredeterminadoAsync(int cursoId);
    }
}
