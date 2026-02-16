using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<UsuarioEntity?> GetByNombreAsync(string nombre);
        Task<bool> CreateAsync(UsuarioEntity usuario);
        Task<bool> UpdatePasswordAsync(string nombre, string newPasswordHash, byte[] versionFila);
        Task<bool> DeleteAsync(string nombre, byte[] versionFila);
        Task<bool> UpdateAdminStatusAsync(string nombre, int isAdmin, byte[] versionFila);
        Task<IEnumerable<UsuarioEntity>> GetAllAsync();
    }
}
