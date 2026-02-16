using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UserService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<UserDto?> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var usuario = await _usuarioRepository.GetByNombreAsync(username);
            if (usuario == null)
                return null;

            return new UserDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Password = null,  // No exponer password en consultas
                EsAdministrador= usuario.EsAdministrador,
                GestionFamilias = usuario.GestionFamilias,
                ConsultaFamilias = usuario.ConsultaFamilias,
                GestionGastos = usuario.GestionGastos,
                ConsultaGastos = usuario.ConsultaGastos,
                VersionFila=usuario.VersionFila
                // Policies no se incluyen aquí, se obtienen por separado
            };
        }


        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            return usuarios.Select(u => new UserDto
            {
                UsuarioId = u.UsuarioId,
                Nombre = u.Nombre,
                Password = null,  // No exponer password
                EsAdministrador = u.EsAdministrador,
                GestionFamilias = u.GestionFamilias,
                ConsultaFamilias = u.ConsultaFamilias,
                GestionGastos = u.GestionGastos,
                ConsultaGastos = u.ConsultaGastos,
                VersionFila = u.VersionFila
            });
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto registerDto)
        {
            // Validar que el usuario no exista
            var existingUser = await _usuarioRepository.GetByNombreAsync(registerDto.Username);
            if (existingUser != null)
            {
                return (false, "El usuario ya existe");
            }

            // Generar hash de la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Crear entidad de usuario
            var usuario = new UsuarioEntity
            {
                Nombre = registerDto.Username,
                Password = passwordHash,
                EsAdministrador= 0
            };

            // Intentar crear el usuario
            var created = await _usuarioRepository.CreateAsync(usuario);
            if (created)
            {
                return (true, "Usuario registrado exitosamente");
            }
            else
            {
                return (false, "Error al registrar el usuario");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.GetByNombreAsync(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para cambiar contraseñas");
            }

            // Verificar que el usuario a cambiar exista
            var targetUsuario = await _usuarioRepository.GetByNombreAsync(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a cambiar no existe");
            }

            // Verificar que las contraseñas coincidan
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return (false, "Las contraseñas no coinciden");
            }

            // Generar hash de la nueva contraseña
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Actualizar la contraseña
            var updated = await _usuarioRepository.UpdatePasswordAsync(dto.Username, newPasswordHash, targetUsuario.VersionFila);
            if (updated)
            {
                return (true, "Contraseña actualizada exitosamente");
            }
            else
            {
                return (false, "Error al actualizar la contraseña");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(string username, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.GetByNombreAsync(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para eliminar usuarios");
            }

            // Verificar que el usuario a eliminar exista
            var targetUsuario = await _usuarioRepository.GetByNombreAsync(username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a eliminar no existe");
            }

            // Verificar que no se elimine a sí mismo
            if (currentUser == username)
            {
                return (false, "No puedes eliminarte a ti mismo");
            }

            // Eliminar el usuario
            var deleted = await _usuarioRepository.DeleteAsync(username, targetUsuario.VersionFila);
            if (deleted)
            {
                return (true, "Usuario eliminado exitosamente");
            }
            else
            {
                return (false, "Error al eliminar el usuario");
            }
        }

        public async Task<(bool Success, string Message)> ChangeAdminStatusAsync(ChangeAdminStatusDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.GetByNombreAsync(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para cambiar el estado de administrador");
            }

            // Verificar que el usuario a cambiar exista
            var targetUsuario = await _usuarioRepository.GetByNombreAsync(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a cambiar no existe");
            }

            // Verificar que no se quite permisos a sí mismo
            if (currentUser == dto.Username && dto.IsAdmin == 0)
            {
                return (false, "No puedes quitarte los permisos de administrador a ti mismo");
            }

            // Actualizar el estado de administrador
            var updated = await _usuarioRepository.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, targetUsuario.VersionFila);
            if (updated)
            {
                return (true, "Estado de administrador actualizado exitosamente");
            }
            else
            {
                return (false, "Error al actualizar el estado de administrador");
            }
        }
    }

}
