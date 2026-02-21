using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IFamiliaService
    {
        Task<FamiliaDto?> LeerPorId(int id);
        Task<bool> EsEliminable(int id);
        Task<IEnumerable<FamiliaDto>> LeerTodos();
        Task<(bool Success, FamiliaDto? Familia)> Crear(RegistrarFamiliaDto dto, string usuarioActual);
        Task<(bool Success,FamiliaDto? Familia)> Actualizar(CambiarFamiliaDto dto, string usuarioActual);
        Task<bool> Eliminar(int familiaId, byte[] versionFila, string usuarioActual);
        Task<IEnumerable<FamiliaHistoriaDto>> LeerHistoria(int id);
        Task<IEnumerable<FamiliaDto>> LeerFiltrado(FilterFamiliaOptions[] filters);
        IEnumerable<FamiliaFieldDto> ObtenerCamposDisponibles();
    }
}
