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
        Task<UsuarioEntity?> CreateAsync(UsuarioEntity usuario, string usuarioActual);
        Task<bool> UpdatePasswordAsync(string nombre, string newPasswordHash, byte[] versionFila, string usuarioActual);
        Task<bool> DeleteAsync(string nombre, byte[] versionFila);
        Task<bool> UpdateAdminStatusAsync(string nombre, int isAdmin, byte[] versionFila, string usuarioActual);
        Task<bool> UpdateActivStatusAsync(string nombre, int isActiv, byte[] versionFila, string usuarioActual);
        Task<bool> UpdateRolStatusAsync(string nombre, int gestionFamilias, int consultaFamilias,
             int gestionGastos, int consultaGastos, byte[] versionFila, string usuarioActual);
        Task<IEnumerable<UsuarioEntity>> GetAllAsync();
    }
}
