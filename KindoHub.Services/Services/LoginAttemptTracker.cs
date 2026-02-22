using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class LoginAttemptTracker
    {
        private readonly ConcurrentDictionary<string, LoginAttemptInfo> _failedAttempts = new();
        private readonly ILogger<LoginAttemptTracker> _logger;
        private const int TIEMPO_BLOQUEO_PRIMERA_VEZ = 5; // minutos
        private const int TIEMPO_BLOQUEO_VECES_SUBSIGUIENTES = 40; // minutos
        private const string USUARIO_QUE_BLOQUEA = "system"; 
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public LoginAttemptTracker(ILogger<LoginAttemptTracker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void RecordFailedAttempt(string username)
        {
            _failedAttempts.AddOrUpdate(
                username,
                new LoginAttemptInfo
                {
                    FailedAttempts = 1,
                    LastAttempt = DateTime.UtcNow,
                    BlockCount = 0
                },
                (key, existing) =>
                {
                    existing.FailedAttempts++;
                    existing.LastAttempt = DateTime.UtcNow;

                    if (existing.FailedAttempts > 2 && !existing.IsBlocked)
                    {
                        existing.IsBlocked = true;
                        existing.BlockCount++;
                        existing.BlockStartTime = DateTime.UtcNow;

                        var blockDuration = existing.BlockCount == 1 ? TIEMPO_BLOQUEO_PRIMERA_VEZ : TIEMPO_BLOQUEO_VECES_SUBSIGUIENTES;
                        _logger.LogWarning($"Usuario '{username}' bloqueado. Bloqueo #{existing.BlockCount}. Duración: {blockDuration} minutos. Total intentos fallidos: {existing.FailedAttempts}");

                        if (existing.BlockCount >= 3)
                        {
                            // Ejecutar el bloqueo en base de datos de forma asíncrona sin bloquear
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    using var scope = _serviceScopeFactory.CreateScope();
                                    var usuarioService = scope.ServiceProvider.GetRequiredService<IUsuarioService>();
                                    var bloquearUsuario = new CambiarEstadoActivoDto
                                    {
                                        Username = username,
                                        IsActive = 0
                                    };
                                    await usuarioService.CambiarEstadoActivo(bloquearUsuario, USUARIO_QUE_BLOQUEA);

                                    _logger.LogCritical($"Usuario '{username}' ha alcanzado {existing.BlockCount} bloqueos. Usuario deshabilitado en base de datos.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Error al deshabilitar usuario '{username}' en base de datos.");
                                }
                            });
                        }
                    }

                    return existing;
                });
        }

        public void ResetAttempts(string username)
        {
            _failedAttempts.TryRemove(username, out _);
        }

        public bool IsUserBlocked(string username)
        {
            if (_failedAttempts.TryGetValue(username, out var info))
            {
                if (info.FailedAttempts > 2 && info.IsBlocked)
                {
                    var blockDuration = info.BlockCount == 1 ? TIEMPO_BLOQUEO_PRIMERA_VEZ : TIEMPO_BLOQUEO_VECES_SUBSIGUIENTES;
                    var timeSinceBlock = DateTime.UtcNow - info.BlockStartTime;

                    if (timeSinceBlock.TotalMinutes < blockDuration)
                    {
                        return true;
                    }
                    else
                    {
                        info.IsBlocked = false;
                        info.FailedAttempts = 0;
                        _logger.LogInformation($"Usuario '{username}' desbloqueado automáticamente después de {blockDuration} minutos. Bloqueo #{info.BlockCount} completado.");
                        return false;
                    }
                }
            }
            return false;
        }

        public int GetFailedAttempts(string username)
        {
            return _failedAttempts.TryGetValue(username, out var info) ? info.FailedAttempts : 0;
        }
    }

    public class LoginAttemptInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime LastAttempt { get; set; }
        public int BlockCount { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime BlockStartTime { get; set; }
    }
}
