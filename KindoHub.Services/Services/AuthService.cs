using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public AuthService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<(bool IsValid, string[] Roles, string[] Permissions)> ValidateUserAsync(LoginDto loginDto)
        {
            var usuario = await _usuarioRepository.GetByNombreAsync(loginDto.Username);
            if (usuario == null || string.IsNullOrEmpty(usuario.Password))
            {
                return (false, Array.Empty<string>(), Array.Empty<string>());
            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password);
            if (!isValidPassword)
            {
                return (false, Array.Empty<string>(), Array.Empty<string>());
            }

            var roles = usuario.EsAdministrador == 1
                ? new[] { "Administrator" }
                : Array.Empty<string>();

            var permissions = Array.Empty<string>();
            //var permissions = await _usuarioRepository.GetPoliticasAsync(loginDto.Username);

            return (true, roles, permissions);
        }
    }
}
