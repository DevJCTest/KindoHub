using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioDto?> GetUserAsync(string username);
        Task<IEnumerable<UsuarioDto>> GetAllUsersAsync();
        Task<(bool Success, string Message, UsuarioDto? User)> RegisterAsync(RegistrarUsuarioDto registerDto, string currentUser);
        Task<(bool Success, string Message, UsuarioDto? User)> ChangePasswordAsync(CambiarContrasenaDto dto, string currentUser);
        Task<(bool Success, string Message)> DeleteUserAsync(EliminarUsuarioDto dto, string currentUser);
        Task<(bool Success, string Message, UsuarioDto? User)> ChangeAdminStatusAsync(CambiarEstadoAdminDto dto, string currentUser);
        Task<(bool Success, string Message, UsuarioDto? User)> ChangeActivStatusAsync(CambiarEstadoActivoDto dto, string currentUser);
        Task<(bool Success, string Message, UsuarioDto? User)> ChangeRolStatusAsync(CambiarRolUsuarioDto dto, string currentUser);
    }
}
