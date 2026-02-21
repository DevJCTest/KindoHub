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
        Task<CursoEntity?> LeerPorId(int id);
        Task<CursoEntity?> LeerPorNombre(string nombre);
        Task<IEnumerable<CursoEntity>> LeerTodos();
        Task<CursoEntity?> LeerPredeterminado();
        Task<CursoEntity?> Crear(CursoEntity curso, string usuarioActual);
        Task<bool> Actualizar(CursoEntity curso, string usuarioActual);
        Task<bool> Eliminar(int id, byte[] versionFila, string usuarioActual);
        Task<bool> EstablecerPredeterminado(int id, string usuarioActual);
        Task<IEnumerable<CursoHistoriaEntitiy>> LeerHistoria(int id);
    }
}
