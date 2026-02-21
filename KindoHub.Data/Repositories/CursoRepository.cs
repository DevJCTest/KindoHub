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
    public class CursoRepository : ICursoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<CursoRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public CursoRepository(IDbConnectionFactoryFactory factory, ILogger<CursoRepository> logger)
        {
            _connectionFactory = factory.Crear("DefaultConnection");
            _logger = logger;
        }

        public async Task<CursoEntity?> LeerPorId(int cursoId)
        {
            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado, VersionFila
            FROM Cursos
            WHERE CursoId = @CursoId";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return CursoMapper.MapToCursoEntity(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer curso: {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<IEnumerable<CursoEntity>> LeerTodos()
        {
            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado, VersionFila
            FROM Cursos";

            var cursos = new List<CursoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cursos.Add(CursoMapper.MapToCursoEntity(reader));
                }

                return cursos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer todos los cursos");
                throw;
            }
        }

        public async Task<CursoEntity?> LeerPredeterminado()
        {
            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado, VersionFila
            FROM Cursos
            WHERE Predeterminado = 1";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    return CursoMapper.MapToCursoEntity(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer el curso predeterminado");
                throw;
            }
        }

        public async Task<CursoEntity?> Crear(CursoEntity curso, string usuarioActual)
        {
            const string query = @"
            INSERT INTO Cursos (Nombre, Descripcion, Predeterminado, CreadoPor, ModificadoPor)
            OUTPUT INSERTED.CursoId
            VALUES (@Nombre, @Descripcion, @Predeterminado, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", curso.Nombre);
                command.Parameters.AddWithValue("@Descripcion", curso.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Predeterminado", curso.Predeterminado);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    curso.Id = Convert.ToInt32(result);
                    return await LeerPorId(curso.Id);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear curso: {Nombre}", curso.Nombre);
                throw;
            }
        }

        public async Task<bool> Actualizar(CursoEntity curso, string usuarioActual)
        {
            const string query = @"
            UPDATE Cursos
            SET Nombre = @Nombre,
                Descripcion = @Descripcion,
                ModificadoPor = @UsuarioActual,
                FechaModificacion = GETDATE()
            WHERE CursoId = @CursoId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", curso.Id);
                command.Parameters.AddWithValue("@Nombre", curso.Nombre);
                command.Parameters.AddWithValue("@Descripcion", curso.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                command.Parameters.AddWithValue("@VersionFila", curso.VersionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar curso: {CursoId}", curso.Id);
                throw;
            }
        }

        public async Task<bool> Eliminar(int cursoId, byte[] versionFila, string usuarioActual)
        {
            const string query = @"
                        BEGIN TRANSACTION;
                        BEGIN TRY

                            UPDATE Cursos
                            SET ModificadoPor = @UsuarioActual,
                                FechaModificacion = GETDATE()
                            WHERE CursoId = @CursoId 
                              AND VersionFila = @VersionFila;


                            IF @@ROWCOUNT = 0
                            BEGIN
                                ROLLBACK TRANSACTION;
                                SELECT 0 AS Result;
                                RETURN;
                            END

                            DELETE FROM Cursos
                            WHERE CursoId = @CursoId;

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
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "Error de clave foránea al eliminar curso: {CursoId}", cursoId);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar curso: {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<bool> EstablecerPredeterminado(int cursoId, string usuarioActual)
        {
            const string query = @"
            BEGIN TRANSACTION;

            BEGIN TRY
                UPDATE Cursos
                SET 
                    ModificadoPor = @UsuarioActual,
                    FechaModificacion = GETDATE(),
                    Predeterminado = 0 
                WHERE
                    Predeterminado=1;
                
                UPDATE Cursos
                SET 
                    Predeterminado = 1,
                    ModificadoPor = @UsuarioActual,
                    FechaModificacion = GETDATE()
                WHERE CursoId = @CursoId;
                
                IF @@ROWCOUNT = 0
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
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteScalarAsync();

                return result != null && Convert.ToInt32(result) == 1;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al establecer curso predeterminado: {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<IEnumerable<CursoHistoriaEntitiy>> LeerHistoria(int id)
        {
            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado, CreadoPor, FechaCreacion, ModificadoPor,
                    FechaModificacion, VersionFila, SysStartTime, SysEndTime
            FROM Cursos_History
            WHERE
            CursoId=@CursoId";

            var cursos = new List<CursoHistoriaEntitiy>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", id);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cursos.Add(CursoMapper.MapToCursoHistoryEntity(reader));
                }

                return cursos;
            }
            catch (SqlException ex)
            {
               _logger.LogError(ex, "Error SQL al leer el historial del curso: {CursoId}", id);
                throw;
            }
        }
    }
}
