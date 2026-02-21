using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface ICursoService
    {
        Task<CursoDto?> LeerPorId(int id);
        Task<CursoDto?> LeerPorNombre(string nombre);
        Task<bool> EsEliminable(int id);
        Task<IEnumerable<CursoDto>> LeerTodos();
        Task<IEnumerable<CursoHistoriaDto>> LeerHistoria(int id);
        Task<CursoDto?> LeerPredeterminado();
        Task<(bool Success, CursoDto? Curso)> Crear(RegistrarCursoDto dto, string usuarioActual);
        Task<(bool Success, CursoDto? Curso)> Actualizar(ActualizarCursoDto dto, string usuarioActual);
        Task<bool> Eliminar(int id, byte[] versionFila, string usuarioActual);
        Task<(bool Success, CursoDto? Curso)> EstablecerPredeterminado(int id, string usuarioActual);
    }
}
