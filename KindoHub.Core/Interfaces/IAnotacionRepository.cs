using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAnotacionRepository
    {
        Task<AnotacionEntity?> LeerPorId(int id);
        Task<IEnumerable<AnotacionEntity>> LeerPorFamilia(int idFamilia);
        Task<AnotacionEntity?> Crear(AnotacionEntity anotacion, string usuarioActual);
        Task<bool> Actualizar(AnotacionEntity anotacion, string usuarioActual);
        Task<bool> Eliminar(int id, byte[] versionFila, string usuarioActual);
        Task<IEnumerable<AnotacionHistoriaEntity>> LeerHistoria(int id);
    }
}
