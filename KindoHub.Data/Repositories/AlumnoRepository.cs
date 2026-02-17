using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
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
            _connectionFactory = factory.Create("DefaultConnection");
            _logger = logger;
        }

        public async Task<AlumnoEntity?> GetByIdAsync(int alumnoId)
        {
            if (alumnoId <= 0)
                throw new ArgumentException("El AlumnoId debe ser mayor a 0", nameof(alumnoId));

            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            WHERE AlumnoId = @AlumnoId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AlumnoId", alumnoId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapearAlumno(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener alumno: {AlumnoId}", alumnoId);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> GetAllAsync()
        {
            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            ORDER BY Nombre ASC";

            var alumnos = new List<AlumnoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(MapearAlumno(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener todos los alumnos");
                throw;
            }
        }

        public async Task<AlumnoEntity?> CreateAsync(AlumnoEntity alumno, string usuarioActual)
        {
            if (alumno == null)
                throw new ArgumentNullException(nameof(alumno));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));

            const string query = @"
            INSERT INTO Alumnos (IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso, CreadoPor, ModificadoPor)
            OUTPUT INSERTED.AlumnoId
            VALUES (@IdFamilia, @Nombre, @Observaciones, @AutorizaRedes, @IdCurso, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
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
                    return await GetByIdAsync(alumno.AlumnoId);
                }

                return null;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "FK violation al crear alumno: IdFamilia={IdFamilia}, IdCurso={IdCurso}",
                    alumno.IdFamilia, alumno.IdCurso);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear alumno: {Nombre}", alumno.Nombre);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(AlumnoEntity alumno, string usuarioActual)
        {
            if (alumno == null)
                throw new ArgumentNullException(nameof(alumno));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));
            if (alumno.VersionFila == null || alumno.VersionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida", nameof(alumno));

            const string query = @"
            UPDATE Alumnos
            SET IdFamilia = @IdFamilia,
                Nombre = @Nombre,
                Observaciones = @Observaciones,
                AutorizaRedes = @AutorizaRedes,
                IdCurso = @IdCurso,
                ModificadoPor = @UsuarioActual
            WHERE AlumnoId = @AlumnoId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
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
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "FK violation al actualizar alumno: {AlumnoId}", alumno.AlumnoId);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar alumno: {AlumnoId}", alumno.AlumnoId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int alumnoId, byte[] versionFila)
        {
            if (alumnoId <= 0)
                throw new ArgumentException("El AlumnoId debe ser mayor a 0", nameof(alumnoId));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida", nameof(versionFila));

            const string query = @"
            DELETE FROM Alumnos
            WHERE AlumnoId = @AlumnoId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AlumnoId", alumnoId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "FK violation al eliminar alumno: {AlumnoId}", alumnoId);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar alumno: {AlumnoId}", alumnoId);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> GetByFamiliaIdAsync(int familiaId)
        {
            if (familiaId <= 0)
                throw new ArgumentException("El FamiliaId debe ser mayor a 0", nameof(familiaId));

            const string query = @"
            SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
                   CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
            FROM Alumnos
            WHERE IdFamilia = @FamiliaId
            ORDER BY Nombre ASC";

            var alumnos = new List<AlumnoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FamiliaId", familiaId);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(MapearAlumno(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener alumnos de familia: {FamiliaId}", familiaId);
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> GetSinFamiliaAsync()
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
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(MapearAlumno(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener alumnos sin familia");
                throw;
            }
        }

        public async Task<IEnumerable<AlumnoEntity>> GetByCursoIdAsync(int cursoId)
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
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    alumnos.Add(MapearAlumno(reader));
                }

                return alumnos;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener alumnos de curso: {CursoId}", cursoId);
                throw;
            }
        }

        public async Task<int> CountByFamiliaIdAsync(int familiaId)
        {
            if (familiaId <= 0)
                throw new ArgumentException("El FamiliaId debe ser mayor a 0", nameof(familiaId));

            const string query = @"
            SELECT COUNT(*)
            FROM Alumnos
            WHERE IdFamilia = @FamiliaId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FamiliaId", familiaId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al contar alumnos de familia: {FamiliaId}", familiaId);
                throw;
            }
        }

        public async Task<int> CountByCursoIdAsync(int cursoId)
        {
            if (cursoId <= 0)
                throw new ArgumentException("El CursoId debe ser mayor a 0", nameof(cursoId));

            const string query = @"
            SELECT COUNT(*)
            FROM Alumnos
            WHERE IdCurso = @CursoId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CursoId", cursoId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al contar alumnos de curso: {CursoId}", cursoId);
                throw;
            }
        }

        private AlumnoEntity MapearAlumno(SqlDataReader reader)
        {
            return new AlumnoEntity
            {
                AlumnoId = reader.GetInt32(0),
                IdFamilia = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Nombre = reader.GetString(2),
                Observaciones = reader.IsDBNull(3) ? null : reader.GetString(3),
                AutorizaRedes = reader.GetBoolean(4),
                IdCurso = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                CreadoPor = reader.GetString(6),
                FechaCreacion = reader.GetDateTime(7),
                ModificadoPor = reader.IsDBNull(8) ? null : reader.GetString(8),
                FechaModificacion = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                VersionFila = (byte[])reader[10]
            };
        }
    }
}
