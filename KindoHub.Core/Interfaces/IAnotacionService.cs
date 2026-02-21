using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAnotacionService
    {
        Task<AnotacionDto?> LeerPorId(int id);
        Task<IEnumerable<AnotacionDto>> LeerPorIdFamilia(int idFamilia);
        Task<(bool Success, AnotacionDto? Anotacion)> Crear(RegistrarAnotacionDto dto, string usuarioActual);
        Task<(bool Success,  AnotacionDto? Anotacion)> Actualizar(Actualizar dto, string usuarioActual);
        Task<bool> Eliminar(int anotacionId, byte[] versionFila, string usuarioActual);
        Task<IEnumerable<AnotacionHistoriaDto>> LeerHistoria(int id);
    }
}
