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
        Task<FamiliaEntity?> GetByFamiliaIdAsync(int familiaId);
        Task<FamiliaEntity?> CreateAsync(FamiliaEntity familia, string usuarioActual);
        Task<bool> UpdateFamiliaAsync(FamiliaEntity familia, string usuarioActual);
        Task<bool> DeleteAsync(int familiaId, byte[] versionFila);
        Task<IEnumerable<FamiliaEntity>> GetAllAsync();
    }
}
