using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IFamiliaRepository
    {
        Task<FamiliaEntity?> LeerPorId(int id);
        Task<FamiliaEntity?> Crear(FamiliaEntity familia, string usuarioActual);
        Task<bool> Actualizar(FamiliaEntity familia, string usuarioActual);
        Task<bool> Eliminar(int familiaId, byte[] versionFila, string usuarioActual);
        Task<IEnumerable<FamiliaEntity>> LeerTodos();
        Task<IEnumerable<FamiliaHistoriaEntity>> LeerHistoria(int id);
    }
}
