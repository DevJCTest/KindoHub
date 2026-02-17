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
    public class CursoRepository : ICursoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<CursoRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public CursoRepository(IDbConnectionFactoryFactory factory, ILogger<CursoRepository> logger)
        {
            _connectionFactory = factory.Create("DefaultConnection");
            _logger = logger;
        }

        public async Task<CursoEntity?> GetByIdAsync(int cursoId)
        {
            if (cursoId <= 0)
                throw new ArgumentException("El identificador del curso debe ser mayor a 0", nameof(cursoId));

            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Cursos
            WHERE CursoId = @CursoId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CursoEntity
                    {
                        CursoId = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Predeterminado = reader.GetBoolean(3),
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
                _logger.LogError(ex, "Error SQL al obtener curso: {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<IEnumerable<CursoEntity>> GetAllAsync()
        {
            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Cursos
            ORDER BY 
                CASE WHEN Predeterminado = 1 THEN 0 ELSE 1 END,
                Nombre ASC";

            var cursos = new List<CursoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cursos.Add(new CursoEntity
                    {
                        CursoId = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Predeterminado = reader.GetBoolean(3),
                        CreadoPor = reader.GetString(4),
                        FechaCreacion = reader.GetDateTime(5),
                        ModificadoPor = reader.IsDBNull(6) ? null : reader.GetString(6),
                        FechaModificacion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        VersionFila = (byte[])reader[8]
                    });
                }

                return cursos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener todos los cursos");
                throw;
            }
        }

        public async Task<CursoEntity?> GetPredeterminadoAsync()
        {
            const string query = @"
            SELECT CursoId, Nombre, Descripcion, Predeterminado
            FROM Cursos
            WHERE Predeterminado = 1";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                
                CursoEntity? curso = null;
                int count = 0;

                while (await reader.ReadAsync())
                {
                    count++;
                    if (count > 1)
                    {
                        _logger.LogError("Se encontraron múltiples cursos predeterminados. Esto indica un problema de integridad de datos.");
                        throw new InvalidOperationException("Hay más de un curso marcado como predeterminado");
                    }

                    curso = new CursoEntity
                    {
                        CursoId = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Predeterminado = reader.GetBoolean(3)
                    };
                }

                return curso;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener curso predeterminado");
                throw;
            }
        }

        public async Task<CursoEntity?> CreateAsync(CursoEntity curso, string usuarioActual)
        {
            if (curso == null)
                throw new ArgumentNullException(nameof(curso));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));

            const string query = @"
            INSERT INTO Cursos (Nombre, Descripcion, Predeterminado, CreadoPor, ModificadoPor)
            OUTPUT INSERTED.CursoId
            VALUES (@Nombre, @Descripcion, @Predeterminado, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", curso.Nombre);
                command.Parameters.AddWithValue("@Descripcion", curso.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Predeterminado", curso.Predeterminado);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    curso.CursoId = Convert.ToInt32(result);
                    return await GetByIdAsync(curso.CursoId);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear curso: {Nombre}", curso.Nombre);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(CursoEntity curso, string usuarioActual)
        {
            if (curso == null)
                throw new ArgumentNullException(nameof(curso));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));
            if (curso.VersionFila == null || curso.VersionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida", nameof(curso));

            const string query = @"
            UPDATE Cursos
            SET Nombre = @Nombre,
                Descripcion = @Descripcion,
                ModificadoPor = @UsuarioActual
            WHERE CursoId = @CursoId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", curso.CursoId);
                command.Parameters.AddWithValue("@Nombre", curso.Nombre);
                command.Parameters.AddWithValue("@Descripcion", curso.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                command.Parameters.AddWithValue("@VersionFila", curso.VersionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar curso: {CursoId}", curso.CursoId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int cursoId, byte[] versionFila)
        {
            if (cursoId <= 0)
                throw new ArgumentException("El identificador del curso debe ser mayor a 0", nameof(cursoId));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida", nameof(versionFila));

            const string query = @"
            DELETE FROM Cursos
            WHERE CursoId = @CursoId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);

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

        public async Task<bool> SetPredeterminadoAsync(int cursoId)
        {
            if (cursoId <= 0)
                throw new ArgumentException("El identificador del curso debe ser mayor a 0", nameof(cursoId));

            const string query = @"
            BEGIN TRANSACTION;

            BEGIN TRY
                UPDATE Cursos
                SET Predeterminado = 0;
                
                UPDATE Cursos
                SET Predeterminado = 1
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
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                var result = await command.ExecuteScalarAsync();

                return result != null && Convert.ToInt32(result) == 1;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al establecer curso predeterminado: {CursoId}", cursoId);
                throw;
            }
        }
    }
}
