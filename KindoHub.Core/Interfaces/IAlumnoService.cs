using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAlumnoService
    {
        Task<AlumnoDto?> LeerPorId(int id);
        Task<IEnumerable<AlumnoDto>> LeerTodos();
        Task<(bool Success, AlumnoDto? Alumno)> Crear(RegistrarAlumnoDto dto, string usuarioActual);
        Task<(bool Success, AlumnoDto? Alumno)> Actualizar(ActualizarAlumnoDto dto, string usuarioActual);
        Task<bool> Eliminar(int alumnoId, byte[] versionFila, string usuarioActual);

        Task<IEnumerable<AlumnoDto>> LeerPorFamiliaId(int familiaId);
        Task<IEnumerable<AlumnoDto>> LeerSinFamilia();
        Task<IEnumerable<AlumnoDto>> LeerPorCursoId(int cursoId);
        Task<IEnumerable<AlumnoHistoriaDto>> LeerHistoria(int id);

        Task<IEnumerable<AlumnoDto>> GetPorFamiliaId(int id);

        Task<IEnumerable<AlumnoDto>> LeerFiltrado(FilterAlumnoOptions[] filters);
        IEnumerable<AlumnoFieldDto> ObtenerCamposDisponibles();

    }
}
