using KindoHub.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioDto?> LeerPorNombre(string username);
        Task<IEnumerable<UsuarioDto>> LeerTodos();
        Task<(bool Success,  UsuarioDto? User)> Registrar(RegistrarUsuarioDto registerDto, string currentUser);
        Task<(bool Success, UsuarioDto? User)> CambiarContraseña(CambiarContrasenaDto dto, string currentUser);
        Task<bool>  Eliminar(EliminarUsuarioDto dto, string currentUser);
        Task<(bool Success, UsuarioDto? User)> CambiarEstadoAdmin(CambiarEstadoAdminDto dto, string currentUser);
        Task<(bool Success, UsuarioDto? User)> CambiarEstadoActivo(CambiarEstadoActivoDto dto, string currentUser);
        Task<(bool Success, UsuarioDto? User)> CambiarRol(CambiarRolUsuarioDto dto, string currentUser);
    }
}
