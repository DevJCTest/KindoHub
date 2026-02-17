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
    public class AnotacionRepository : IAnotacionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<AnotacionRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public AnotacionRepository(IDbConnectionFactoryFactory factory, ILogger<AnotacionRepository> logger)
        {
            _connectionFactory = factory.Create("DefaultConnection");
            _logger = logger;
        }

        public async Task<AnotacionEntity?> GetByIdAsync(int anotacionId)
        {
            if (anotacionId <= 0)
                throw new ArgumentException("El identificador de la anotación debe ser mayor a 0", nameof(anotacionId));

            const string query = @"
            SELECT AnotacionId, IdFamilia, Fecha, Descripcion, 
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Anotaciones
            WHERE AnotacionId = @AnotacionId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnotacionId", anotacionId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new AnotacionEntity
                    {
                        AnotacionId = reader.GetInt32(0),
                        IdFamilia = reader.GetInt32(1),
                        Fecha = reader.GetDateTime(2),
                        Descripcion = reader.GetString(3),
                        CreadoPor = reader.GetString(4),
                        FechaCreacion = reader.GetDateTime(5),
                        ModificadoPor = reader.IsDBNull(6) ? null : reader.GetString(6),
                        FechaModificacion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        VersionFila = (byte[])reader[8]
                    };
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener anotación: {AnotacionId}", anotacionId);
                throw;
            }
        }

        public async Task<IEnumerable<AnotacionEntity>> GetByFamiliaIdAsync(int idFamilia)
        {
            if (idFamilia <= 0)
                throw new ArgumentException("El identificador de la familia debe ser mayor a 0", nameof(idFamilia));

            const string query = @"
            SELECT AnotacionId, IdFamilia, Fecha, Descripcion, 
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Anotaciones
            WHERE IdFamilia = @IdFamilia
            ORDER BY Fecha DESC, AnotacionId DESC";

            var anotaciones = new List<AnotacionEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IdFamilia", idFamilia);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    anotaciones.Add(new AnotacionEntity
                    {
                        AnotacionId = reader.GetInt32(0),
                        IdFamilia = reader.GetInt32(1),
                        Fecha = reader.GetDateTime(2),
                        Descripcion = reader.GetString(3),
                        CreadoPor = reader.GetString(4),
                        FechaCreacion = reader.GetDateTime(5),
                        ModificadoPor = reader.IsDBNull(6) ? null : reader.GetString(6),
                        FechaModificacion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        VersionFila = (byte[])reader[8]
                    });
                }

                return anotaciones;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener anotaciones de familia: {IdFamilia}", idFamilia);
                throw;
            }
        }

        public async Task<AnotacionEntity?> CreateAsync(AnotacionEntity anotacion, string usuarioActual)
        {
            if (anotacion == null)
                throw new ArgumentNullException(nameof(anotacion));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario que realiza la acción no puede estar vacío.", nameof(usuarioActual));

            const string query = @"
            INSERT INTO Anotaciones (IdFamilia, Fecha, Descripcion, CreadoPor, ModificadoPor)
            OUTPUT INSERTED.AnotacionId
            VALUES (@IdFamilia, @Fecha, @Descripcion, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IdFamilia", anotacion.IdFamilia);
                command.Parameters.AddWithValue("@Fecha", anotacion.Fecha);
                command.Parameters.AddWithValue("@Descripcion", anotacion.Descripcion);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    anotacion.AnotacionId = Convert.ToInt32(result);
                    return await GetByIdAsync(anotacion.AnotacionId);
                }

                return null;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogWarning("Intento de crear anotación con familia inexistente: {IdFamilia}", anotacion.IdFamilia);
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear anotación para familia: {IdFamilia}", anotacion.IdFamilia);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(AnotacionEntity anotacion, string usuarioActual)
        {
            if (anotacion == null)
                throw new ArgumentNullException(nameof(anotacion));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario que realiza la acción no puede estar vacío.", nameof(usuarioActual));
            if (anotacion.VersionFila == null || anotacion.VersionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(anotacion));

            const string query = @"
            UPDATE Anotaciones
            SET IdFamilia = @IdFamilia,
                Fecha = @Fecha,
                Descripcion = @Descripcion,
                ModificadoPor = @UsuarioActual
            WHERE AnotacionId = @AnotacionId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnotacionId", anotacion.AnotacionId);
                command.Parameters.AddWithValue("@IdFamilia", anotacion.IdFamilia);
                command.Parameters.AddWithValue("@Fecha", anotacion.Fecha);
                command.Parameters.AddWithValue("@Descripcion", anotacion.Descripcion);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                command.Parameters.AddWithValue("@VersionFila", anotacion.VersionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "Error de clave foránea al actualizar anotación: {AnotacionId}", anotacion.AnotacionId);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar anotación: {AnotacionId}", anotacion.AnotacionId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int anotacionId, byte[] versionFila)
        {
            if (anotacionId <= 0)
                throw new ArgumentException("El identificador de la anotación debe ser mayor a 0", nameof(anotacionId));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));

            const string query = @"
            DELETE FROM Anotaciones
            WHERE AnotacionId = @AnotacionId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnotacionId", anotacionId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "Error de clave foránea al eliminar anotación: {AnotacionId}", anotacionId);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar anotación: {AnotacionId}", anotacionId);
                throw;
            }
        }
    }
}
