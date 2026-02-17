using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Repositories
{
    public class EstadoAsociadoRepository:IEstadoAsociadoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<EstadoAsociadoRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public EstadoAsociadoRepository(IDbConnectionFactoryFactory factory, ILogger<EstadoAsociadoRepository> logger)
        {
            _connectionFactory = factory.Create("DefaultConnection");
            _logger = logger;
        }

        public async Task<IEnumerable<EstadoAsociadoEntity>> GetAllEstadoAsociadoAsync()
        {
            _logger.LogDebug("Obteniendo todos los estados asociados");

            const string query = @"
            SELECT EstadoId, Nombre, Descripcion
            FROM EstadosAsociado
            ORDER BY Nombre";

            var estadoAsociado = new List<EstadoAsociadoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    estadoAsociado.Add(new EstadoAsociadoEntity
                    {
                        EstadoAsociadoId = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Descripcion = reader.GetString(reader.GetOrdinal("Descripcion"))
                    });
                }

                _logger.LogInformation("Se obtuvieron {Count} estados asociados", estadoAsociado.Count);
                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener los estados asociados");
                throw;
            }
        }

        public async Task<EstadoAsociadoEntity?> GetEstadoAsociadoAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del estado asociado no puede estar vacío.", nameof(nombre));

            _logger.LogDebug("Buscando estado asociado: {Nombre}", nombre);

            const string query = @"
            SELECT EstadoId, Nombre, Descripcion
            FROM EstadosAsociado
            WHERE Nombre = @Nombre";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    _logger.LogDebug("Estado asociado no encontrado: {Nombre}", nombre);
                    return null;
                }

                var estadoAsociado = new EstadoAsociadoEntity
                {
                    EstadoAsociadoId = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion"))
                };

                _logger.LogInformation("Estado asociado encontrado: {EstadoId} - {Nombre}", estadoAsociado.EstadoAsociadoId, estadoAsociado.Nombre);
                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar estado asociado: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<EstadoAsociadoEntity?> GetEstadoAsociadoAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del estado asociado no puede ser menor o igual a cero.", nameof(id));
            _logger.LogDebug("Buscando estado asociado: {EstadoAsociadoId}", id);
            const string query = @"
            SELECT EstadoId, Nombre, Descripcion
            FROM EstadosAsociado
            WHERE EstadoId = @EstadoAsociadoId";
            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EstadoAsociadoId", id);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    _logger.LogDebug("Estado asociado no encontrado: {EstadoAsociadoId}", id);
                    return null;
                }

                var estadoAsociado = new EstadoAsociadoEntity
                {
                    EstadoAsociadoId = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion"))
                };

                _logger.LogInformation("Estado asociado encontrado: {EstadoId} - {Nombre}", estadoAsociado.EstadoAsociadoId, estadoAsociado.Nombre);
                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar estado asociado: {EstadoId}", id);
                throw;
            }
        }
    }
}
