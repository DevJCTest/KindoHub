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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KindoHub.Data.Repositories
{
    public class FamiliaRepository : IFamiliaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<FamiliaRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public FamiliaRepository(IDbConnectionFactoryFactory factory, ILogger<FamiliaRepository> logger)
        {
            _connectionFactory = factory.Create("DefaultConnection");
            _logger = logger;
        }


        public async Task<FamiliaEntity?> CreateAsync(FamiliaEntity familia, string usuarioActual)
        {
            if (familia == null)
                throw new ArgumentNullException(nameof(familia));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario que realiza la acción no puede estar vacío.", nameof(usuarioActual));


            const string query = @"
            INSERT INTO familias (NumeroSocio, Nombre, Email, Telefono, Direccion,
                                Observaciones, Apa, IdEstadoApa, Mutual, IdEstadoMutual, BeneficiarioMutual, 
                                IdFormaPago, Iban, CreadoPor, ModificadoPor)
            OUTPUT INSERTED.FamiliaId
            VALUES (@NumeroSocio, @Nombre, @Email, @Telefono, @Direccion,
                    @Observaciones, @Apa, @IdEstadoApa, @Mutual, @IdEstadoMutual, @BeneficiarioMutual,
                    @IdFormaPago, @Iban, @UsuarioActual, @UsuarioActual)";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NumeroSocio", familia.NumeroSocio);
                command.Parameters.AddWithValue("@Nombre", familia.Nombre);
                command.Parameters.AddWithValue("@Email", familia.Email);
                command.Parameters.AddWithValue("@Telefono", familia.Telefono);
                command.Parameters.AddWithValue("@Direccion", familia.Direccion);
                command.Parameters.AddWithValue("@Observaciones", familia.Observaciones);
                command.Parameters.AddWithValue("@Apa", familia.Apa);
                command.Parameters.AddWithValue("@IdEstadoApa", familia.IdEstadoApa ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Mutual", familia.Mutual);
                command.Parameters.AddWithValue("@IdEstadoMutual", familia.IdEstadoMutual?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BeneficiarioMutual", familia.BeneficiarioMutual);
                command.Parameters.AddWithValue("@IdFormaPago", familia.IdFormaPago ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Iban", familia.IBAN);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);



                var result = await command.ExecuteScalarAsync();


                if (result != null && result != DBNull.Value)
                {
                    familia.FamiliaId = Convert.ToInt32(result);
                    return await GetByFamiliaIdAsync(familia.FamiliaId);
                }

                return null;
            }
            catch (SqlException ex) when (ex.Number == SqlUniqueConstraintViolation)
            {
                _logger.LogWarning("Intento de crear familia duplicado: {Nombre}", familia.Nombre);
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear familia: {Nombre}", familia.Nombre);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int familiaId, byte[] versionFila)
        {
            if (familiaId<=0)
                throw new ArgumentException("El identificador de la familia ha de ser mayor o igual a 0", nameof(familiaId));
            if (versionFila == null || versionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(versionFila));


            const string query = @"
            DELETE FROM familias
            WHERE FamiliaId = @FamiliaId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FamiliaId", familiaId);
                command.Parameters.AddWithValue("@VersionFila", versionFila);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                _logger.LogError(ex, "Error de clave foránea al eliminar familia: {FamiliaId}", familiaId);
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al eliminar familia: {FamiliaId}", familiaId);
                throw;
            }
        }

        public async Task<IEnumerable<FamiliaEntity>> GetAllAsync()
        {
            const string query = @"
            SELECT [FamiliaId]
                ,[NumeroSocio]
                ,f.[Nombre]
                ,[Email]
                ,[Telefono]
                ,[Direccion]
                ,[Observaciones]
                ,[Apa]
                ,[IdEstadoApa]
                ,estApa.Nombre EstadoApa
                ,[Mutual]
                ,[IdEstadoMutual]
                ,estMutual.Nombre EstadoMutual
                ,[BeneficiarioMutual]
                ,[IdFormaPago]
                ,forPago.Descripcion FormaPago
                ,[IBAN]
                ,[IBAN_Enmascarado]
                ,[CreadoPor]
                ,[FechaCreacion]
                ,[ModificadoPor]
                ,[FechaModificacion]
                ,[VersionFila]
                ,[SysStartTime]
                ,[SysEndTime]
            FROM [KindoHub].[dbo].[Familias] f
            left join EstadosAsociado estApa on estApa.EstadoId=f.IdEstadoApa
            left join EstadosAsociado estMutual on estMutual.EstadoId=f.IdEstadoMutual
            left join FormasPago forPago on forPago.FormaPagoId=f.IdFormaPago
            ORDER BY FamiliaId";

            var familias = new List<FamiliaEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    familias.Add(FamiliaMapper.MapToFamiliaEntity(reader));
                }

                _logger.LogInformation("Se obtuvieron {Count} familias", familias.Count);
                return familias;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener todas las familias");
                throw;
            }
        }

        public async Task<FamiliaEntity?> GetByFamiliaIdAsync(int familiaId)
        {
            if (familiaId <= 0)
                throw new ArgumentException("El identificador de la familia ha de ser mayor o igual a 0", nameof(familiaId));

            _logger.LogDebug("Buscando familia: {FamiliaId}", familiaId);

            const string query = @"
            SELECT [FamiliaId]
                ,[NumeroSocio]
                ,f.[Nombre]
                ,[Email]
                ,[Telefono]
                ,[Direccion]
                ,[Observaciones]
                ,[Apa]
                ,[IdEstadoApa]
                ,estApa.Nombre EstadoApa
                ,[Mutual]
                ,[IdEstadoMutual]
                ,estMutual.Nombre EstadoMutual
                ,[BeneficiarioMutual]
                ,[IdFormaPago]
                ,forPago.Descripcion FormaPago
                ,[IBAN]
                ,[IBAN_Enmascarado]
                ,[CreadoPor]
                ,[FechaCreacion]
                ,[ModificadoPor]
                ,[FechaModificacion]
                ,[VersionFila]
                ,[SysStartTime]
                ,[SysEndTime]
            FROM [KindoHub].[dbo].[Familias] f
            left join EstadosAsociado estApa on estApa.EstadoId=f.IdEstadoApa
            left join EstadosAsociado estMutual on estMutual.EstadoId=f.IdEstadoMutual
            left join FormasPago forPago on forPago.FormaPagoId=f.IdFormaPago
            WHERE FamiliaId = @FamiliaId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FamiliaId", familiaId);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    _logger.LogDebug("Familia no encontrada: {FamiliaId}", familiaId);
                    return null;
                }

                var familia=FamiliaMapper.MapToFamiliaEntity(reader);

                //var familia = new FamiliaEntity
                //{
                //    FamiliaId = reader.GetInt32(reader.GetOrdinal("FamiliaId")),
                //    NumeroSocio =  reader.IsDBNull(reader.GetOrdinal("NumeroSocio"))
                //          ? 0 : reader.GetInt32(reader.GetOrdinal("NumeroSocio")),
                //    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                //    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                //    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("Telefono")),
                //    Direccion= reader.IsDBNull(reader.GetOrdinal("Direccion"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("Direccion")),
                //    Observaciones= reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("Observaciones")),
                //    Apa = reader.GetBoolean(reader.GetOrdinal("Apa")),
                //    IdEstadoApa = reader.IsDBNull(reader.GetOrdinal("IdEstadoApa"))
                //          ? 0: reader.GetInt32(reader.GetOrdinal("IdEstadoApa")),
                //    NombreEstadoApa = reader.IsDBNull(reader.GetOrdinal("EstadoApa"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("EstadoApa")),
                //    Mutual = reader.GetBoolean(reader.GetOrdinal("Mutual")),
                //    IdEstadoMutual= reader.IsDBNull(reader.GetOrdinal("IdEstadoMutual"))
                //          ? 0 : reader.GetInt32(reader.GetOrdinal("IdEstadoMutual")),
                //    NombreEstadoMutual= reader.IsDBNull(reader.GetOrdinal("EstadoMutual"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("EstadoMutual")),
                //    BeneficiarioMutual = reader.GetBoolean(reader.GetOrdinal("BeneficiarioMutual")),
                //    IdFormaPago = reader.IsDBNull(reader.GetOrdinal("IdFormaPago"))
                //          ? 0 : reader.IsDBNull(reader.GetOrdinal("IdFormaPago"))
                //          ? null : reader.GetInt32(reader.GetOrdinal("IdFormaPago")),
                //    NombreFormaPago = reader.IsDBNull(reader.GetOrdinal("FormaPago"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("FormaPago")),
                //    IBAN= reader.IsDBNull(reader.GetOrdinal("Iban"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("Iban")),
                //    IBAN_Enmascarado= reader.IsDBNull(reader.GetOrdinal("Iban_Enmascarado"))
                //          ? string.Empty : reader.GetString(reader.GetOrdinal("Iban_Enmascarado")),
                //    CreadoPor = reader.GetString(reader.GetOrdinal("CreadoPor")),
                //    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                //    ModificadoPor= reader.GetString(reader.GetOrdinal("ModificadoPor")),
                //    FechaModificacion = reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                //    VersionFila = reader.IsDBNull(reader.GetOrdinal("VersionFila"))
                //        ? Array.Empty<byte>()
                //        : (byte[])reader["VersionFila"]
                //};

                _logger.LogInformation("Familia encontrada: {FamiliaId} - {Nombre}", familia.FamiliaId, familia.Nombre);
                return familia;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar usuario: {FamiliaId}",familiaId);
                throw;
            }
        }

        public async Task<bool> UpdateFamiliaAsync(FamiliaEntity familia, string usuarioActual)
        {
            if (familia == null)
                throw new ArgumentNullException(nameof(familia));
            if (string.IsNullOrWhiteSpace(familia.Nombre))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(familia.Nombre));
            if (familia.VersionFila == null || familia.VersionFila.Length == 0)
                throw new ArgumentException("VersionFila es requerida para concurrencia optimista.", nameof(familia.VersionFila));
            if (string.IsNullOrWhiteSpace(usuarioActual))
                throw new ArgumentException("El usuario actual no puede estar vacío.", nameof(usuarioActual));

            _logger.LogInformation("Actualizando familia {FamiliaId} por {ModificadoPor}",
                familia.FamiliaId, usuarioActual);

            const string query = @"
            UPDATE Familias
            SET NumeroSocio=@NumeroSocio, Nombre=@Nombre, Email=@Email, Telefono=@Telefono, Direccion=@Direccion,
                Observaciones=@Observaciones, Apa=@Apa, IdEstadoApa=@IdEstadoApa, Mutual=@Mutual, 
                IdEstadoMutual=@IdEstadoMutual, BeneficiarioMutual=@BeneficiarioMutual, IdFormaPago=@IdFormaPago,
                Iban=@Iban, ModificadoPor=@UsuarioActual, FechaModificacion=GETDATE()
            WHERE FamiliaId=@FamiliaId AND VersionFila = @VersionFila";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NumeroSocio", familia.NumeroSocio);
                command.Parameters.AddWithValue("@Nombre", familia.Nombre);
                command.Parameters.AddWithValue("@Email", familia.Email);
                command.Parameters.AddWithValue("@Telefono", familia.Telefono);
                command.Parameters.AddWithValue("@Direccion", familia.Direccion);
                command.Parameters.AddWithValue("@Observaciones", familia.Observaciones);
                command.Parameters.AddWithValue("@Apa", familia.Apa);
                command.Parameters.AddWithValue("@IdEstadoApa", familia.IdEstadoApa ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Mutual", familia.Mutual);
                command.Parameters.AddWithValue("@IdEstadoMutual", familia.IdEstadoMutual ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BeneficiarioMutual", familia.BeneficiarioMutual);
                command.Parameters.AddWithValue("@IdFormaPago", familia.IdFormaPago ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Iban", familia.IBAN ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                command.Parameters.AddWithValue("@FamiliaId", familia.FamiliaId);
                command.Parameters.AddWithValue("@VersionFila", familia.VersionFila);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    _logger.LogInformation("Estado activo actualizado exitosamente para: {FamiliaId}", familia.FamiliaId);
                else
                    _logger.LogWarning("No se pudo actualizar estado activo (posible conflicto): {FamiliaId}", familia.FamiliaId);

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al actualizar estado activo para: {FamiliaId}", familia.FamiliaId);
                throw;
            }
        }
    }
}
