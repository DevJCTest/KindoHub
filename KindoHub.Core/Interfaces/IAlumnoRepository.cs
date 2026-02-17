using KindoHub.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAlumnoRepository
    {
        Task<AlumnoEntity?> GetByIdAsync(int alumnoId);
        Task<IEnumerable<AlumnoEntity>> GetAllAsync();
        Task<AlumnoEntity?> CreateAsync(AlumnoEntity alumno, string usuarioActual);
        Task<bool> UpdateAsync(AlumnoEntity alumno, string usuarioActual);
        Task<bool> DeleteAsync(int alumnoId, byte[] versionFila);

        Task<IEnumerable<AlumnoEntity>> GetByFamiliaIdAsync(int familiaId);
        Task<IEnumerable<AlumnoEntity>> GetSinFamiliaAsync();
        Task<IEnumerable<AlumnoEntity>> GetByCursoIdAsync(int cursoId);

        Task<int> CountByFamiliaIdAsync(int familiaId);
        Task<int> CountByCursoIdAsync(int cursoId);
    }
}
