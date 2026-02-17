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
        Task<AnotacionEntity?> GetByIdAsync(int anotacionId);
        Task<IEnumerable<AnotacionEntity>> GetByFamiliaIdAsync(int idFamilia);
        Task<AnotacionEntity?> CreateAsync(AnotacionEntity anotacion, string usuarioActual);
        Task<bool> UpdateAsync(AnotacionEntity anotacion, string usuarioActual);
        Task<bool> DeleteAsync(int anotacionId, byte[] versionFila);
    }
}
