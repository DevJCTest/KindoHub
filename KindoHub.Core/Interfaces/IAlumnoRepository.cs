using KindoHub.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAlumnoRepository
    {
        Task<AlumnoEntity?> LeerPorId(int id);
        Task<IEnumerable<AlumnoEntity>> LeerTodos();
        Task<IEnumerable<AlumnoEntity>> LeerFiltrado(FilterAlumnoOptions[] filters);

        Task<AlumnoEntity?> Crear(AlumnoEntity alumno, string usuarioActual);
        Task<bool> Actualizar(AlumnoEntity alumno, string usuarioActual);
        Task<bool> Eliminar(int alumnoId, byte[] versionFila, string usuarioActual);

        Task<IEnumerable<AlumnoEntity>> LeerPorFamiliaId(int familiaId);
        Task<IEnumerable<AlumnoEntity>> LeerSinFamilia();
        Task<IEnumerable<AlumnoEntity>> LeerPorCursoId(int cursoId);

        Task<int> NumeroPorFamiliaId(int familiaId);
        Task<int> NumeroPorCursoId(int cursoId);
        Task<IEnumerable<AlumnoHistoriaEntity>> LeerHistoria(int id);
    }
}
