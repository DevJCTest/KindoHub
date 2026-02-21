using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Transformers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class UserService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUsuarioRepository usuarioRepository, ILogger<UserService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }



        public async Task<UsuarioDto?> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var usuario = await _usuarioRepository.LeerPorNombre(username);
            if (usuario == null)
                return null;

            return UserMapper.MapToUserDto(usuario);

        }


        public async Task<IEnumerable<UsuarioDto>> GetAllUsersAsync()
        {
            var usuarios = await _usuarioRepository.LeerTodos();
            return usuarios.Select(u => UserMapper.MapToUserDto(u));
        }

        public async Task<(bool Success, string Message, UsuarioDto? User)> RegisterAsync(RegistrarUsuarioDto registerDto, string currentUser)
        {
            _logger.LogInformation("Iniciando registro de usuario: {Username} por {CurrentUser}", 
                registerDto.Username, currentUser);

            // Validar que el usuario no exista
            var existingUser = await _usuarioRepository.LeerPorNombre(registerDto.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de registro de usuario existente: {Username}", registerDto.Username);
                return (false, "El usuario ya existe", null);
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
            var createdUser = await _usuarioRepository.CreateAsync(usuario, currentUser);
            if (createdUser != null)
            {
                _logger.LogInformation("Usuario registrado exitosamente: {Username} con ID: {UsuarioId}", 
                    createdUser.Nombre, createdUser.UsuarioId);

                return (true, "Usuario registrado exitosamente", UserMapper.MapToUserDto(createdUser));
            }
            else
            {
                _logger.LogError("Error al registrar usuario: {Username}", registerDto.Username);
                return (false, "Error al registrar el usuario", null);
            }
        }

        public async Task<(bool Success, string Message, UsuarioDto? User)> ChangePasswordAsync(CambiarContrasenaDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.LeerPorNombre(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para cambiar contraseñas", null);
            }

            // Verificar que el usuario a cambiar exista
            var targetUsuario = await _usuarioRepository.LeerPorNombre(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a cambiar no existe", null);
            }

            // Verificar que las contraseñas coincidan
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return (false, "Las contraseñas no coinciden", null);
            }

            // Generar hash de la nueva contraseña
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Actualizar la contraseña
            var updated = await _usuarioRepository.UpdatePasswordAsync(dto.Username, newPasswordHash, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true, "Contraseña actualizada exitosamente", UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false, "Error al actualizar la contraseña", null);
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(EliminarUsuarioDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.LeerPorNombre(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para eliminar usuarios");
            }

            // Verificar que el usuario a eliminar exista
            var targetUsuario = await _usuarioRepository.LeerPorNombre(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a eliminar no existe");
            }

            // Verificar que no se elimine a sí mismo
            if (currentUser == dto.Username)
            {
                return (false, "No puedes eliminarte a ti mismo");
            }


            var updates = await _usuarioRepository.ActualizarEstadoAdmin(currentUsuario.Nombre, currentUsuario.EsAdministrador, currentUsuario.VersionFila, currentUser);


            // Eliminar el usuario
            var deleted = await _usuarioRepository.DeleteAsync(dto.Username, dto.VersionFila);
            if (deleted)
            {
                return (true, "Usuario eliminado exitosamente");
            }
            else
            {
                return (false, "Error al eliminar el usuario");
            }
        }

        public async Task<(bool Success, string Message, UsuarioDto? User)> ChangeAdminStatusAsync(CambiarEstadoAdminDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.LeerPorNombre(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para cambiar el estado de administrador", null);
            }

            // Verificar que el usuario a cambiar exista
            var targetUsuario = await _usuarioRepository.LeerPorNombre(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a cambiar no existe", null);
            }

            // Verificar que no se quite permisos a sí mismo
            if (currentUser == dto.Username && dto.IsAdmin == 0)
            {
                return (false, "No puedes quitarte los permisos de administrador a ti mismo", null);
            }

            // Actualizar el estado de administrador
            var updated = await _usuarioRepository.ActualizarEstadoAdmin(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true, "Estado de administrador actualizado exitosamente", UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false, "Error al actualizar el estado de administrador", null);
        }

        public async Task<(bool Success, string Message, UsuarioDto? User)> ChangeActivStatusAsync(CambiarEstadoActivoDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.LeerPorNombre(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para cambiar el estado de administrador", null);
            }

            // Verificar que el usuario a cambiar exista
            var targetUsuario = await _usuarioRepository.LeerPorNombre(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a cambiar no existe", null);
            }


            // Actualizar el estado de activo
            var updated = await _usuarioRepository.ActualizarEstadoActivo(dto.Username, dto.IsActive, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true, "Estado de usuario actualizado exitosamente", UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false, "Error al actualizar el estado del usuario", null);
        }

        public async Task<(bool Success, string Message, UsuarioDto? User)> ChangeRolStatusAsync(CambiarRolUsuarioDto dto, string currentUser)
        {
            // Verificar que el usuario actual sea administrador
            var currentUsuario = await _usuarioRepository.LeerPorNombre(currentUser);
            if (currentUsuario == null || currentUsuario.EsAdministrador != 1)
            {
                return (false, "No tienes permisos para cambiar el rol a los usuarios", null);
            }

            // Verificar que el usuario a cambiar exista
            var targetUsuario = await _usuarioRepository.LeerPorNombre(dto.Username);
            if (targetUsuario == null)
            {
                return (false, "El usuario a cambiar no existe", null);
            }


            // Actualizar el rol del usuario
            var updated = await _usuarioRepository.ActualizarEstadoRol(dto.Username, dto.GestionFamilias, dto.ConsultaFamilias, 
                dto.GestionGastos, dto.ConsultaGastos, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true, "Rol de usuario actualizado exitosamente", UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false, "Error al actualizar el rol del usuario", null);
        }
    }

}
