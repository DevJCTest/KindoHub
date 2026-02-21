using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Data.Transformers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Data.Repositories
{
    public class AlumnoRepository : IAlumnoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<AlumnoRepository> _logger;
        private const int SqlForeignKeyViolation = 547;

        public AlumnoRepository(IDbConnectionFactoryFactory factory, ILogger<AlumnoRepository> logger)
        {
            _connectionFactory = factory.Crear("DefaultConnection");
            _logger = logger;
        }

        public async Task<AlumnoEntity?> LeerPorId(int id)
        {
            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso, VersionFila
            FROM Alumnos
            WHERE AlumnoId = @AlumnoId";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AlumnoId", id);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return AlumnoMapper.MapToAlumnoEntity(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer el alumno con id {AlumnoId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> LeerTodos()
        {
            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            ORDER BY Nombre ASC";

            var alumnos = new List<AlumnoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(AlumnoMapper.MapToAlumnoEntity(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer todos los alumnos");
                throw;
            }
        }

        public async Task<AlumnoEntity?> Crear(AlumnoEntity alumno, string usuarioActual)
        {
            const string query = @"
            INSERT INTO Alumnos (IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso, CreadoPor, ModificadoPor)
            OUTPUT INSERTED.AlumnoId
            VALUES (@IdFamilia, @Nombre, @Observaciones, @AutorizaRedes, @IdCurso, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@IdFamilia", alumno.IdFamilia ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombre", alumno.Nombre);
                command.Parameters.AddWithValue("@Observaciones", alumno.Observaciones ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AutorizaRedes", alumno.AutorizaRedes);
                command.Parameters.AddWithValue("@IdCurso", alumno.IdCurso ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    alumno.AlumnoId = Convert.ToInt32(result);
                    return await LeerPorId(alumno.AlumnoId);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear el alumno: {Nombre}", alumno.Nombre);
                throw;
            }
        }

        public async Task<bool> Actualizar(AlumnoEntity alumno, string usuarioActual)
        {
            const string query = @"
            UPDATE Alumnos
            SET IdFamilia = @IdFamilia,
                Nombre = @Nombre,
                Observaciones = @Observaciones,
                AutorizaRedes = @AutorizaRedes,
                IdCurso = @IdCurso,
                ModificadoPor = @UsuarioActual,
                FechaModificacion = GETDATE()
            WHERE AlumnoId = @AlumnoId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@AlumnoId", alumno.AlumnoId);
                command.Parameters.AddWithValue("@IdFamilia", alumno.IdFamilia ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombre", alumno.Nombre);
                command.Parameters.AddWithValue("@Observaciones", alumno.Observaciones ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AutorizaRedes", alumno.AutorizaRedes);
                command.Parameters.AddWithValue("@IdCurso", alumno.IdCurso ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                command.Parameters.AddWithValue("@VersionFila", alumno.VersionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar el alumno con id {AlumnoId}", alumno.AlumnoId);
                throw;
            }
        }

        public async Task<bool> Eliminar(int alumnoId, byte[] versionFila, string usuarioActual)
        {
            const string query = @"
            BEGIN TRANSACTION;
                        BEGIN TRY

                            UPDATE Alumnos
                            SET ModificadoPor = @UsuarioActual,
                                FechaModificacion = GETDATE()
                            WHERE AlumnoId = @AlumnoId 
                              AND VersionFila = @VersionFila;


                            IF @@ROWCOUNT = 0
                            BEGIN
                                ROLLBACK TRANSACTION;
                                SELECT 0 AS Result;
                                RETURN;
                            END

                            DELETE FROM Alumnos
                            WHERE AlumnoId= @AlumnoId;

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
                command.Parameters.AddWithValue("@AlumnoId", alumnoId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar el alumno con id {AlumnoId}", alumnoId);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> LeerPorFamiliaId(int familiaId)
        {
            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            WHERE IdFamilia = @FamiliaId
            ORDER BY Nombre ASC";

            var alumnos = new List<AlumnoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FamiliaId", familiaId);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(AlumnoMapper.MapToAlumnoEntity(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer los alumnos de la familia con id {FamiliaId}", familiaId);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> LeerSinFamilia()
        {
            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            WHERE IdFamilia IS NULL OR IdFamilia = 0
            ORDER BY Nombre ASC";

            var alumnos = new List<AlumnoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(AlumnoMapper.MapToAlumnoEntity(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer los alumnos sin familia");
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> LeerPorCursoId(int cursoId)
        {
            if (cursoId <= 0)
                throw new ArgumentException("El CursoId debe ser mayor a 0", nameof(cursoId));

            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            WHERE IdCurso = @CursoId
            ORDER BY Nombre ASC";

            var alumnos = new List<AlumnoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(AlumnoMapper.MapToAlumnoEntity(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener alumnos del curso con id {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<int> NumeroPorFamiliaId(int familiaId)
        {
            const string query = @"
            SELECT COUNT(*)
            FROM Alumnos
            WHERE IdFamilia = @FamiliaId";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FamiliaId", familiaId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al contar los alumnos de familia: {FamiliaId}", familiaId);
                throw;
            }
        }

        public async Task<int> NumeroPorCursoId(int cursoId)
        {
            const string query = @"
            SELECT COUNT(*)
            FROM Alumnos
            WHERE IdCurso = @CursoId";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al contar los alumnos del curso con id {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoHistoriaEntity>> LeerHistoria(int id)
        {
            const string query = @"
            SELECT [AlumnoId]
                      ,[IdFamilia]
                      ,[Nombre]
                      ,[Observaciones]
                      ,[AutorizaRedes]
                      ,[IdCurso]
                      ,[CreadoPor]
                      ,[FechaCreacion]
                      ,[ModificadoPor]
                      ,[FechaModificacion]
                      ,[VersionFila]
                      ,[SysStartTime]
                      ,[SysEndTime]
                  FROM [Alumnos_History] 
                    WHERE
                       AlumnoId=@AlumnoId";

            var alumnos = new List<AlumnoHistoriaEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AlumnoId", id);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(AlumnoMapper.MapToAlumnoHistoriaEntity(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al leer la historia del alumno con id {Id}", id);
                throw;
            }
        }
    }
}
