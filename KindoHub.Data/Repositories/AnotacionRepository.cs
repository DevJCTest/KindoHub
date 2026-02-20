using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Data.Transformers;
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

        public async Task<AnotacionEntity?> LeerPorId(int anotacionId)
        {
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
                    return AnotacionMapper.MapToAnotacionEntity(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener anotación: {AnotacionId}", anotacionId);
                throw;
            }
        }

        public async Task<IEnumerable<AnotacionEntity>> LeerPorFamilia(int idFamilia)
        {
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
                    anotaciones.Add(AnotacionMapper.MapToAnotacionEntity(reader));
                }

                return anotaciones;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener anotaciones de la familia: {IdFamilia}", idFamilia);
                throw;
            }
        }

        public async Task<AnotacionEntity?> Crear(AnotacionEntity anotacion, string usuarioActual)
        {
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
                    anotacion.Id = Convert.ToInt32(result);
                    return await LeerPorId(anotacion.Id);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear anotación para familia: {IdFamilia}", anotacion.IdFamilia);
                throw;
            }
        }

        public async Task<bool> Actualizar(AnotacionEntity anotacion, string usuarioActual)
        {
            const string query = @"
            UPDATE Anotaciones
            SET 
                Fecha = @Fecha,
                Descripcion = @Descripcion,
                ModificadoPor = @UsuarioActual
            WHERE AnotacionId = @AnotacionId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnotacionId", anotacion.Id);
                command.Parameters.AddWithValue("@Fecha", anotacion.Fecha);
                command.Parameters.AddWithValue("@Descripcion", anotacion.Descripcion);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                command.Parameters.AddWithValue("@VersionFila", anotacion.VersionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar anotación: {AnotacionId}", anotacion.Id);
                throw;
            }
        }

        public async Task<bool> Eliminar(int anotacionId, byte[] versionFila, string usuarioActual)
        {
            const string query = @"
                                BEGIN TRANSACTION;
                    BEGIN TRY

                        UPDATE Anotaciones
                        SET ModificadoPor = @UsuarioActual,
                            FechaModificacion = GETDATE()
                        WHERE AnotacionId = @AnotacionId 
                            AND VersionFila = @VersionFila;


                        IF @@ROWCOUNT = 0
                        BEGIN
                            ROLLBACK TRANSACTION;
                            SELECT 0 AS Result;
                            RETURN;
                        END

                        DELETE FROM Anotaciones
                        WHERE AnotacionId = @AnotacionId;

                        DECLARE @FilasBorradas INT = @@ROWCOUNT;

                        IF @FilasBorradas = 0
                        BEGIN
                            ROLLBACK TRANSACTION;
                            SELECT 0 AS Result;
                            RETURN;
                        END

                        COMMIT TRANSACTION;
                        SELECT 1 AS Result;

                    END TRY
                    BEGIN CATCH
                        ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnotacionId", anotacionId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }

            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar anotación: {AnotacionId}", anotacionId);
                throw;
            }
        }

        public async Task<IEnumerable<AnotacionHistoriaEntity>> LeerHistoria(int id)
        {
            const string query = @"
            SELECT AnotacionId, IdFamilia, Fecha, Descripcion, 
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila, SysStartTime, SysEndTime
            FROM Anotaciones_History
            WHERE AnotacionId = @AnotacionId";

            var anotaciones = new List<AnotacionHistoriaEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnotacionId", id);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    anotaciones.Add(AnotacionMapper.MapToAnotacionHistoriaEntity(reader));
                }

                return anotaciones;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener anotación: {AnotacionId}", id);
                throw;
            }
        }
    }
}
