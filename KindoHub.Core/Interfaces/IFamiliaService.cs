using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
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
        Task<FamiliaDto?> GetByFamiliaIdAsync(int familiaId);
        Task<IEnumerable<FamiliaDto>> GetAllAsync();

        Task<(bool Success, string Message, FamiliaDto? Familia)> CreateAsync(RegisterFamiliaDto dto, string usuarioActual);
        Task<(bool Success, string Message, FamiliaDto? Familia)> UpdateFamiliaAsync(ChangeFamiliaDto dto, string usuarioActual);
        Task<(bool Success, string Message)> DeleteAsync(int familiaId, byte[] versionFila);
    }
}
