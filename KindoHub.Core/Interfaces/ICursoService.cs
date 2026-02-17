using KindoHub.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface ICursoService
    {
        Task<CursoDto?> GetByIdAsync(int cursoId);
        Task<IEnumerable<CursoDto>> GetAllAsync();
        Task<CursoDto?> GetPredeterminadoAsync();
        Task<(bool Success, string Message, CursoDto? Curso)> CreateAsync(RegisterCursoDto dto, string usuarioActual);
        Task<(bool Success, string Message, CursoDto? Curso)> UpdateAsync(UpdateCursoDto dto, string usuarioActual);
        Task<(bool Success, string Message)> DeleteAsync(int cursoId, byte[] versionFila);
        Task<(bool Success, string Message, CursoDto? Curso)> SetPredeterminadoAsync(int cursoId);
    }
}
