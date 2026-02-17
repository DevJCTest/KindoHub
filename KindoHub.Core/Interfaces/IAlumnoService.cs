using KindoHub.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAlumnoService
    {
        Task<AlumnoDto?> GetByIdAsync(int alumnoId);
        Task<IEnumerable<AlumnoDto>> GetAllAsync();
        Task<(bool Success, string Message, AlumnoDto? Alumno)> CreateAsync(RegisterAlumnoDto dto, string usuarioActual);
        Task<(bool Success, string Message, AlumnoDto? Alumno)> UpdateAsync(UpdateAlumnoDto dto, string usuarioActual);
        Task<(bool Success, string Message)> DeleteAsync(int alumnoId, byte[] versionFila);

        Task<IEnumerable<AlumnoDto>> GetByFamiliaIdAsync(int familiaId);
        Task<IEnumerable<AlumnoDto>> GetSinFamiliaAsync();
        Task<IEnumerable<AlumnoDto>> GetByCursoIdAsync(int cursoId);
    }
}
