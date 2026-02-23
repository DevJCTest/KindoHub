using KindoHub.Core.Dtos;
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
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IUsuarioRepository usuarioRepository, ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }



        public async Task<UsuarioDto?> LeerPorNombre(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var usuario = await _usuarioRepository.LeerPorNombre(username);
            if (usuario == null)
                return null;

            return UserMapper.MapToUserDto(usuario);

        }


        public async Task<IEnumerable<UsuarioDto>> LeerTodos()
        {
            var usuarios = await _usuarioRepository.LeerTodos();
            return usuarios.Select(u => UserMapper.MapToUserDto(u));
        }

        public async Task<(bool Success,  UsuarioDto? User)> Registrar(RegistrarUsuarioDto registerDto, string currentUser)
        {
            var existingUser = await _usuarioRepository.LeerPorNombre(registerDto.Username);
            if (existingUser != null)
            {
                return (false,  null);
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
                return (true,  UserMapper.MapToUserDto(createdUser));
            }
            else
            {
                return (false,  null);
            }
        }

        public async Task<(bool Success, UsuarioDto? User)> CambiarContraseña(CambiarContrasenaDto dto, string currentUser)
        {

            // Generar hash de la nueva contraseña
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Actualizar la contraseña
            var updated = await _usuarioRepository.UpdatePasswordAsync(dto.Username, newPasswordHash, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true,  UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false,  null);
        }

        public async Task<bool > Eliminar(EliminarUsuarioDto dto, string currentUser)
        {

            var currentUsuario = await _usuarioRepository.LeerPorNombre(dto.Username);
            var updates = await _usuarioRepository.ActualizarEstadoAdmin(currentUsuario.Nombre, currentUsuario.EsAdministrador, currentUsuario.VersionFila, currentUser);


            // Eliminar el usuario
            var deleted = await _usuarioRepository.DeleteAsync(dto.Username, dto.VersionFila);
            if (deleted)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public async Task<(bool Success, UsuarioDto? User)> CambiarEstadoAdmin(CambiarEstadoAdminDto dto, string currentUser)
        {

            // Actualizar el estado de administrador
            var updated = await _usuarioRepository.ActualizarEstadoAdmin(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true,  UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false, null);
        }

        public async Task<(bool Success,  UsuarioDto? User)> CambiarEstadoActivo(CambiarEstadoActivoDto dto, string currentUser)
        {
            // Actualizar el estado de activo
            var updated = await _usuarioRepository.ActualizarEstadoActivo(dto.Username, dto.IsActive, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true,  UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false,  null);
        }

        public async Task<(bool Success, UsuarioDto? User)> CambiarRol(CambiarRolUsuarioDto dto, string currentUser)
        {

            // Actualizar el rol del usuario
            var updated = await _usuarioRepository.ActualizarEstadoRol(dto.Username, dto.GestionFamilias, dto.ConsultaFamilias, 
                dto.GestionGastos, dto.ConsultaGastos, dto.VersionFila, currentUser);
            if (updated)
            {
                var updatedUser = await _usuarioRepository.LeerPorNombre(dto.Username);
                if (updatedUser != null)
                {
                    return (true, UserMapper.MapToUserDto(updatedUser));
                }
            }

            return (false,  null);
        }

        public async Task<bool> RegistrarAdmin(RegistrarAdminDto dto, string userName)
        {
            var existingUser = await _usuarioRepository.LeerPorNombre(userName);
            if (existingUser != null)
            {
                return (false);
            }

            // Generar hash de la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Crear entidad de usuario
            var usuario = new UsuarioEntity
            {
                Nombre = userName,
                Password = passwordHash,
                EsAdministrador = 1
            };

            // Intentar crear el usuario
            var createdUser = await _usuarioRepository.CreateAsync(usuario, "System");
            if (createdUser != null)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }

}
