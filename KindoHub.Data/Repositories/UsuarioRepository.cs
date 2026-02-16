using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using Microsoft.Data.SqlClient;
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

        public UsuarioRepository(IDbConnectionFactoryFactory factory)
        {
            _connectionFactory = factory.Create("DefaultConnection");
        }

        public async Task<UsuarioEntity?> GetByNombreAsync(string nombre)
        {
            const string query = @"
            SELECT UsuarioId, Nombre, Password, Activo, EsAdministrador, GestionFamilias,
            ConsultaFamilias, GestionGastos, ConsultaGastos, VersionFila 
            FROM usuarios
            WHERE nombre = @Nombre";

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", nombre);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new UsuarioEntity
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
                VersionFila = (byte[])reader["VersionFila"]
            };
        }

        public async Task<bool> CreateAsync(UsuarioEntity usuario)
        {
            const string query = @"
            INSERT INTO usuarios (nombre, password, EsAdministrador, CreadoPor, ModificadoPor)
            VALUES (@Nombre, @Password, @IsAdmin, @usuario, @usuario)";

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            command.Parameters.AddWithValue("@Password", usuario.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", usuario.EsAdministrador);
            command.Parameters.AddWithValue("@usuario", "a");

            try
            {
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                return false; // Usuario ya existe
            }
        }

        public async Task<bool> UpdatePasswordAsync(string nombre, string newPasswordHash, byte[] versionFila  )
        {
            const string query = @"
            UPDATE usuarios
            SET password = @NewPasswordHash
            WHERE nombre = @Nombre and VersionFila=@versionfila";

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@versionfila", versionFila); 
            command.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(string nombre, byte[] versionFila)
        {
            const string query = @"
            DELETE FROM usuarios
            WHERE nombre = @Nombre and VersionFila=@versionfila";

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@versionfila", versionFila);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateAdminStatusAsync(string nombre, int isAdmin, byte[] versionFila)
        {
            const string query = @"
            UPDATE usuarios
            SET EsAdministrador = @IsAdmin
            WHERE nombre = @Nombre";

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@IsAdmin", isAdmin);
            command.Parameters.AddWithValue("@versionfila", versionFila);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<IEnumerable<UsuarioEntity>> GetAllAsync()
        {
            const string query = @"
            SELECT nombre, EsAdministrador
            FROM usuarios
            ORDER BY nombre";

            var usuarios = new List<UsuarioEntity>();

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                usuarios.Add(new UsuarioEntity
                {
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    EsAdministrador = reader.GetInt32(reader.GetOrdinal("esadministrador"))
                });
            }

            return usuarios;
        }
    }

}
