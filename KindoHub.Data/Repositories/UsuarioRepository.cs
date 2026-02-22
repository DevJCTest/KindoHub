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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<UsuarioRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public UsuarioRepository(IDbConnectionFactoryFactory factory, ILogger<UsuarioRepository> logger)
        {
            _connectionFactory = factory.Crear("DefaultConnection");
            _logger = logger;
        }

        public async Task<UsuarioEntity?> LeerPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombre));

            _logger.LogDebug("Buscando usuario: {Nombre}", nombre);

            const string query = @"
            SELECT UsuarioId, Nombre, Password, Activo, EsAdministrador, GestionFamilias,
            ConsultaFamilias, GestionGastos, ConsultaGastos, VersionFila 
            FROM usuarios
            WHERE Nombre = @Nombre";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    _logger.LogDebug("Usuario no encontrado: {Nombre}", nombre);
                    return null;
                }

                var usuario = new UsuarioEntity
                {
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Password = reader.IsDBNull(reader.GetOrdinal("Password"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Password")),
                    Activo = reader.GetInt32(reader.GetOrdinal("Activo")),
                    EsAdministrador = reader.GetInt32(reader.GetOrdinal("EsAdministrador")),
                    GestionFamilias = reader.GetInt32(reader.GetOrdinal("GestionFamilias")),
                    ConsultaFamilias = reader.GetInt32(reader.GetOrdinal("ConsultaFamilias")),
                    GestionGastos = reader.GetInt32(reader.GetOrdinal("GestionGastos")),
                    ConsultaGastos = reader.GetInt32(reader.GetOrdinal("ConsultaGastos")),
                    VersionFila = reader.IsDBNull(reader.GetOrdinal("VersionFila"))
                        ? Array.Empty<byte>()
                        : (byte[])reader["VersionFila"]
                };

                _logger.LogInformation("Usuario encontrado: {UsuarioId} - {Nombre}", usuario.UsuarioId, usuario.Nombre);
                return usuario;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar usuario: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<UsuarioEntity?> CreateAsync(UsuarioEntity usuario, string usuarioActual)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(usuario));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario actual no puede estar vacío.", nameof(usuarioActual));

            _logger.LogInformation("Intentando crear usuario: {Nombre} por {CreadoPor}", usuario.Nombre, usuarioActual);

            const string query = @"
            INSERT INTO usuarios (Nombre, Password, EsAdministrador, CreadoPor, ModificadoPor)
            VALUES (@Nombre, @Password, @IsAdmin, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@Password", usuario.Password ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsAdmin", usuario.EsAdministrador);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Usuario creado exitosamente: {Nombre}", usuario.Nombre);
                    return await LeerPorNombre(usuario.Nombre);
                }

                _logger.LogWarning("No se pudo crear el usuario: {Nombre}", usuario.Nombre);
                return null;
            }
            catch (SqlException ex) when (ex.Number == SqlUniqueConstraintViolation)
            {
                _logger.LogWarning("Intento de crear usuario duplicado: {Nombre}", usuario.Nombre);
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear usuario: {Nombre}", usuario.Nombre);
                throw;
            }
        }

        public async Task<bool> UpdatePasswordAsync(string nombre, string newPasswordHash, byte[] versionFila, string usuarioActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("El hash de password no puede estar vacío.", nameof(newPasswordHash));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario actual no puede estar vacío.", nameof(usuarioActual));

            _logger.LogInformation("Actualizando password de usuario: {Nombre} por {ModificadoPor}", nombre, usuarioActual);

            const string query = @"
            UPDATE usuarios
            SET Password = @NewPasswordHash, ModificadoPor = @UsuarioActual, FechaModificacion = GETDATE()
            WHERE Nombre = @Nombre AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);
                command.Parameters.AddWithValue("@VersionFila", versionFila); 
                command.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    _logger.LogInformation("Password actualizado exitosamente para usuario: {Nombre}", nombre);
                else
                    _logger.LogWarning("No se pudo actualizar password (posible conflicto de concurrencia): {Nombre}", nombre);

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar password para usuario: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string nombre, byte[] versionFila)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombre));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));

            _logger.LogWarning("Intentando eliminar usuario: {Nombre}", nombre);

            const string query = @"
            DELETE FROM usuarios
            WHERE Nombre = @Nombre AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);
                command.Parameters.AddWithValue("@VersionFila", versionFila);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    _logger.LogWarning("Usuario eliminado exitosamente: {Nombre}", nombre);
                else
                    _logger.LogWarning("No se pudo eliminar usuario (posible conflicto de concurrencia): {Nombre}", nombre);

                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "Error de clave foránea al eliminar usuario: {Nombre}", nombre);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar usuario: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<bool> ActualizarEstadoAdmin(string nombre, int isAdmin, byte[] versionFila, string usuarioActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombre));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario actual no puede estar vacío.", nameof(usuarioActual));

            _logger.LogInformation("Actualizando estado administrador de {Nombre} a {IsAdmin} por {ModificadoPor}", 
                nombre, isAdmin, usuarioActual);

            const string query = @"
            UPDATE usuarios
            SET EsAdministrador = @IsAdmin, ModificadoPor = @UsuarioActual, FechaModificacion = GETDATE()
            WHERE Nombre = @Nombre AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);
                command.Parameters.AddWithValue("@IsAdmin", isAdmin);
                command.Parameters.AddWithValue("@VersionFila", versionFila);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    _logger.LogInformation("Estado administrador actualizado exitosamente para: {Nombre}", nombre);
                else
                    _logger.LogWarning("No se pudo actualizar estado administrador (posible conflicto): {Nombre}", nombre);

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar estado administrador para: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioEntity>> LeerTodos()
        {
            _logger.LogDebug("Obteniendo todos los usuarios");

            const string query = @"
            SELECT UsuarioId, Nombre, Activo, EsAdministrador, 
                   GestionFamilias, ConsultaFamilias, GestionGastos, ConsultaGastos, 
                   VersionFila
            FROM usuarios
            ORDER BY Nombre";

            var usuarios = new List<UsuarioEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(new UsuarioEntity
                    {
                        UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Activo = reader.GetInt32(reader.GetOrdinal("Activo")),
                        EsAdministrador = reader.GetInt32(reader.GetOrdinal("EsAdministrador")),
                        GestionFamilias = reader.GetInt32(reader.GetOrdinal("GestionFamilias")),
                        ConsultaFamilias = reader.GetInt32(reader.GetOrdinal("ConsultaFamilias")),
                        GestionGastos = reader.GetInt32(reader.GetOrdinal("GestionGastos")),
                        ConsultaGastos = reader.GetInt32(reader.GetOrdinal("ConsultaGastos")),
                        VersionFila = reader.IsDBNull(reader.GetOrdinal("VersionFila"))
                            ? Array.Empty<byte>()
                            : (byte[])reader["VersionFila"]
                    });
                }

                _logger.LogInformation("Se obtuvieron {Count} usuarios activos", usuarios.Count);
                return usuarios;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener todos los usuarios");
                throw;
            }
        }

        public async Task<bool> ActualizarEstadoActivo(string nombre, int isActiv, byte[] versionFila, string usuarioActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombre));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario actual no puede estar vacío.", nameof(usuarioActual));

            _logger.LogInformation("Actualizando estado activo de {Nombre} a {IsActivo} por {ModificadoPor}", 
                nombre, isActiv, usuarioActual);

            const string query = @"
            UPDATE usuarios
            SET Activo = @Activo, ModificadoPor = @UsuarioActual, FechaModificacion = GETDATE()
            WHERE Nombre = @Nombre AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);
                command.Parameters.AddWithValue("@Activo", isActiv);
                command.Parameters.AddWithValue("@VersionFila", versionFila);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    _logger.LogInformation("Estado activo actualizado exitosamente para: {Nombre}", nombre);
                else
                    _logger.LogWarning("No se pudo actualizar estado activo (posible conflicto): {Nombre}", nombre);

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar estado activo para: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<bool> ActualizarEstadoRol(string nombre, int gestionFamilias, int consultaFamilias, int gestionGastos, int consultaGastos, byte[] versionFila, string usuarioActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombre));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario actual no puede estar vacío.", nameof(usuarioActual));

            _logger.LogInformation("Actualizando roles de usuario {Nombre} por {ModificadoPor}", nombre, usuarioActual);

            const string query = @"
            UPDATE usuarios
            SET GestionFamilias = @GestionFamilias, ConsultaFamilias = @ConsultaFamilias,
            GestionGastos = @GestionGastos, ConsultaGastos = @ConsultaGastos,
            ModificadoPor = @UsuarioActual, FechaModificacion = GETDATE()
            WHERE Nombre = @Nombre AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nombre", nombre);
                command.Parameters.AddWithValue("@GestionFamilias", gestionFamilias);   
                command.Parameters.AddWithValue("@ConsultaFamilias", consultaFamilias);
                command.Parameters.AddWithValue("@GestionGastos", gestionGastos);
                command.Parameters.AddWithValue("@ConsultaGastos", consultaGastos);
                command.Parameters.AddWithValue("@VersionFila", versionFila);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    _logger.LogInformation("Roles actualizados exitosamente para: {Nombre}", nombre);
                else
                    _logger.LogWarning("No se pudieron actualizar roles (posible conflicto): {Nombre}", nombre);

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar roles para: {Nombre}", nombre);
                throw;
            }
        }
    }

}
