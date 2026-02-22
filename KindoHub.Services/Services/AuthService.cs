using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<AuthService> _logger;
        private static readonly ConcurrentDictionary<string, LoginAttemptTracker> _loginAttempts = new();
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(10);

        public AuthService(IUsuarioRepository usuarioRepository, ILogger<AuthService> logger)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(bool IsValid, string[] Roles, string[] Permissions)> ValidarUsuario(LoginDto loginDto)
        {
            try
            {
                // 1. Validación de entrada robusta
                if (loginDto == null)
                {
                    _logger.LogWarning("Login attempt with null LoginDto");
                    return CreateFailedLoginResponse();
                }

                if (string.IsNullOrWhiteSpace(loginDto.Username))
                {
                    _logger.LogWarning("Login attempt with empty username");
                    return CreateFailedLoginResponse();
                }

                if (string.IsNullOrEmpty(loginDto.Password))
                {
                    _logger.LogWarning("Login attempt with empty password for username: {Username}", loginDto.Username);
                    return CreateFailedLoginResponse();
                }

                // 2. Protección contra fuerza bruta - Verificar lockout
                var attemptKey = loginDto.Username.ToLowerInvariant();
                if (IsAccountLockedOut(attemptKey))
                {
                    _logger.LogWarning("Login attempt for locked account. Username: {Username}", loginDto.Username);
                    return CreateFailedLoginResponse();
                }

                // 3. Obtener usuario desde el repositorio
                var usuario = await GetUserSafelyAsync(loginDto.Username);

                // 4. Verificar estado de la cuenta
                if (!IsAccountValid(usuario))
                {
                    // Ejecutar BCrypt dummy para prevenir timing attacks
                    await PerformDummyPasswordVerificationAsync(loginDto.Password);
                    RecordFailedAttempt(attemptKey);
                    _logger.LogWarning("Login attempt for invalid or disabled account. Username: {Username}", loginDto.Username);
                    return CreateFailedLoginResponse();
                }

                // 5. Verificar contraseña (protección contra timing attacks incluida)
                bool isValidPassword = await VerifyPasswordAsync(loginDto.Password, usuario!);

                // 6. Evaluar resultado de autenticación
                if (!isValidPassword)
                {
                    RecordFailedAttempt(attemptKey);
                    _logger.LogWarning("Failed login attempt for username: {Username}. Total attempts: {Attempts}", 
                        loginDto.Username, 
                        GetFailedAttemptCount(attemptKey));
                    return CreateFailedLoginResponse();
                }

                // 7. Login exitoso - Limpiar intentos fallidos
                ClearFailedAttempts(attemptKey);
                _logger.LogInformation("Successful login for username: {Username}, UserId: {UserId}", 
                    loginDto.Username, 
                    usuario.UsuarioId);

                // 8. Construir y retornar roles y permisos
                var (roles, permissions) = BuildUserRolesAndPermissions(usuario);
                return (true, roles, permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login validation for username: {Username}", 
                    loginDto?.Username ?? "unknown");
                return CreateFailedLoginResponse();
            }
        }

        private async Task<UsuarioEntity?> GetUserSafelyAsync(string username)
        {
            try
            {
                return await _usuarioRepository.LeerPorNombre(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user from repository. Username: {Username}", username);
                return null;
            }
        }

        private bool IsAccountValid(UsuarioEntity? usuario)
        {
            // 1. Verificar que el usuario existe
            if (usuario == null)
            {
                return false;
            }

            // 2. Verificar que la cuenta está activa (Activo = 1)
            if (usuario.Activo != 1)
            {
                _logger.LogWarning("User account is inactive. UserId: {UserId}, Activo: {Activo}", 
                    usuario.UsuarioId, 
                    usuario.Activo);
                return false;
            }

            // 3. Verificar que el usuario tiene password configurado
            if (string.IsNullOrEmpty(usuario.Password))
            {
                _logger.LogWarning("User account has no password set. UserId: {UserId}", usuario.UsuarioId);
                return false;
            }

            // Futuras verificaciones pueden incluir:
            // - Fecha de expiración de cuenta
            // - Verificación de email
            // - Verificación 2FA
            // - Verificación de roles mínimos requeridos

            return true;
        }

        private async Task<bool> VerifyPasswordAsync(string password, UsuarioEntity usuario)
        {
            try
            {
                return await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, usuario.Password));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for UserId: {UserId}", usuario.UsuarioId);
                return false;
            }
        }

        private async Task PerformDummyPasswordVerificationAsync(string password)
        {
            // Hash dummy para mantener tiempos de respuesta consistentes
            const string dummyHash = "$2a$11$dummyhashtopreventtimingattacks1234567890123456789012";
            try
            {
                await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, dummyHash));
            }
            catch
            {
                // Ignorar errores en verificación dummy
            }
        }

        private (string[] Roles, string[] Permissions) BuildUserRolesAndPermissions(UsuarioEntity usuario)
        {
            var roles = new List<string>();
            var permissions = new List<string>();

            // Construir roles
            if (usuario.EsAdministrador == 1)
            {
                roles.Add("Administrator");
            }

            // Construir permisos de forma más declarativa
            var permissionMapping = new[]
            {
                (Flag: usuario.GestionFamilias, Permission: "Gestion_Familias"),
                (Flag: usuario.ConsultaFamilias, Permission: "Consulta_Familias"),
                (Flag: usuario.GestionGastos, Permission: "Gestion_Gastos"),
                (Flag: usuario.ConsultaGastos, Permission: "Consulta_Gastos")
            };

            foreach (var (flag, permission) in permissionMapping)
            {
                if (flag == 1)
                {
                    permissions.Add(permission);
                }
            }

            _logger.LogDebug("User {UserId} has {RoleCount} roles and {PermissionCount} permissions", 
                usuario.UsuarioId, 
                roles.Count, 
                permissions.Count);

            return (roles.ToArray(), permissions.ToArray());
        }

        private static (bool IsValid, string[] Roles, string[] Permissions) CreateFailedLoginResponse()
        {
            return (false, Array.Empty<string>(), Array.Empty<string>());
        }

        private bool IsAccountLockedOut(string username)
        {
            if (_loginAttempts.TryGetValue(username, out var tracker))
            {
                CleanupOldAttempts(tracker);

                if (tracker.IsLockedOut)
                {
                    if (DateTime.UtcNow < tracker.LockoutEnd)
                    {
                        return true;
                    }

                    // Lockout expirado, limpiar
                    tracker.IsLockedOut = false;
                    tracker.LockoutEnd = null;
                    tracker.FailedAttempts.Clear();
                }
            }
            return false;
        }

        private void RecordFailedAttempt(string username)
        {
            var tracker = _loginAttempts.GetOrAdd(username, _ => new LoginAttemptTracker());

            lock (tracker)
            {
                CleanupOldAttempts(tracker);
                tracker.FailedAttempts.Add(DateTime.UtcNow);

                if (tracker.FailedAttempts.Count >= MaxFailedAttempts)
                {
                    tracker.IsLockedOut = true;
                    tracker.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    _logger.LogWarning("Account locked out due to too many failed attempts. Username: {Username}, LockoutEnd: {LockoutEnd}", 
                        username, 
                        tracker.LockoutEnd);
                }
            }
        }

        private int GetFailedAttemptCount(string username)
        {
            if (_loginAttempts.TryGetValue(username, out var tracker))
            {
                lock (tracker)
                {
                    CleanupOldAttempts(tracker);
                    return tracker.FailedAttempts.Count;
                }
            }
            return 0;
        }

        private void ClearFailedAttempts(string username)
        {
            if (_loginAttempts.TryGetValue(username, out var tracker))
            {
                lock (tracker)
                {
                    tracker.FailedAttempts.Clear();
                    tracker.IsLockedOut = false;
                    tracker.LockoutEnd = null;
                }
            }
        }

        private void CleanupOldAttempts(LoginAttemptTracker tracker)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(AttemptWindow);
            tracker.FailedAttempts.RemoveAll(attempt => attempt < cutoffTime);
        }

        private class LoginAttemptTracker
        {
            public List<DateTime> FailedAttempts { get; set; } = new();
            public bool IsLockedOut { get; set; }
            public DateTime? LockoutEnd { get; set; }
        }
    }
}
