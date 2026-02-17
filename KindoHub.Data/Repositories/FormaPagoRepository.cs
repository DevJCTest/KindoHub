using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
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
    public class FormaPagoRepository : IFormaPagoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<FormaPagoRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public FormaPagoRepository(IDbConnectionFactoryFactory factory, ILogger<FormaPagoRepository> logger)
        {
            _connectionFactory = factory.Create("DefaultConnection");
            _logger = logger;
        }

        public async Task<IEnumerable<FormaPagoEntity>> GetAllFormasPagoAsync()
        {
            _logger.LogDebug("Obteniendo todos las formas de pago");

            const string query = @"
            SELECT FormaPagoId, Nombre, Descripcion
            FROM FormasPago
            ORDER BY Nombre";

            var formasPago = new List<FormaPagoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    formasPago.Add(new FormaPagoEntity
                    {
                        FormaPagoId = reader.GetInt32(reader.GetOrdinal("FormaPagoId")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Descripcion = reader.GetString(reader.GetOrdinal("Descripcion"))
                    });
                }

                _logger.LogInformation("Se obtuvieron {Count} formas de pago", formasPago.Count);
                return formasPago;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener las formas de pago");
                throw;
            }
        }

        public async Task<FormaPagoEntity?> GetFormaPagoAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la forma de pago no puede estar vacío.", nameof(nombre));

            _logger.LogDebug("Buscando forma de pago: {Nombre}", nombre);

            const string query = @"
            SELECT FormaPagoId, Nombre, Descripcion
            FROM FormasPago
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
                    _logger.LogDebug("Forma de pago no encontrada: {Nombre}", nombre);
                    return null;
                }

                var formaPago = new FormaPagoEntity
                {
                    FormaPagoId = reader.GetInt32(reader.GetOrdinal("FormaPagoId")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Descripcion= reader.GetString(reader.GetOrdinal("Descripcion"))
                };

                _logger.LogInformation("Forma de pago encontrada: {FormaPagoId} - {Nombre}", formaPago.FormaPagoId, formaPago.Nombre);
                return formaPago;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar forma de pago: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<FormaPagoEntity?> GetFormaPagoAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID de la forma de pago no puede ser menor o igual a cero.", nameof(id));
            _logger.LogDebug("Buscando forma de pago: {FormaPagoId}", id);

            const string query = @"
            SELECT FormaPagoId, Nombre, Descripcion
            FROM FormasPago
            WHERE FormaPagoId = @FormaPagoId";

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FormaPagoId", id);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    _logger.LogDebug("Forma de pago no encontrada: {FormaPagoid}", id);
                    return null;
                }

                var formaPago = new FormaPagoEntity
                {
                    FormaPagoId = reader.GetInt32(reader.GetOrdinal("FormaPagoId")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion"))
                };

                _logger.LogInformation("Forma de pago encontrada: {FormaPagoId} - {Nombre}", formaPago.FormaPagoId, formaPago.Nombre);
                return formaPago;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar forma de pago: {FormaPagoId}", id);
                throw;
            }
        }
    }
}
