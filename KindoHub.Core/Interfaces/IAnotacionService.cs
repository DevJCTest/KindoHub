using KindoHub.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAnotacionService
    {
        Task<AnotacionDto?> GetByIdAsync(int anotacionId);
        Task<IEnumerable<AnotacionDto>> GetByFamiliaIdAsync(int idFamilia);
        Task<(bool Success, string Message, AnotacionDto? Anotacion)> CreateAsync(RegisterAnotacionDto dto, string usuarioActual);
        Task<(bool Success, string Message, AnotacionDto? Anotacion)> UpdateAsync(UpdateAnotacionDto dto, string usuarioActual);
        Task<(bool Success, string Message)> DeleteAsync(int anotacionId, byte[] versionFila);
    }
}
