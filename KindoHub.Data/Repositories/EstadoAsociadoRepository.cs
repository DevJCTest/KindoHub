using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Data.Extensions;
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
    public class EstadoAsociadoRepository:IEstadoAsociadoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<EstadoAsociadoRepository> _logger;

        private const int SqlUniqueConstraintViolation = 2627;
        private const int SqlForeignKeyViolation = 547;
        private const int SqlDeadlock = 1205;

        public EstadoAsociadoRepository(IDbConnectionFactoryFactory factory, ILogger<EstadoAsociadoRepository> logger)
        {
            _connectionFactory = factory.Crear("DefaultConnection");
            _logger = logger;
        }

        public async Task<IEnumerable<EstadoAsociadoEntity>> LeerTodos()
        {
            const string query = @"
            SELECT EstadoId, Nombre, Descripcion, Predeterminado 
            FROM EstadosAsociado
            ORDER BY Nombre";

            var estadoAsociado = new List<EstadoAsociadoEntity>();

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    estadoAsociado.Add(EstadoAsociadoMapper.MapToEstadoAsociadoEntity(reader));
                }

                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al obtener los estados asociados");
                throw;
            }
        }

        public async Task<EstadoAsociadoEntity?> LeerPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Nombre no asignado", nameof(nombre));

            const string query = @"
            SELECT EstadoId, Nombre, Descripcion, Predeterminado 
            FROM EstadosAsociado
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
                    return null;
                }

                var estadoAsociado= EstadoAsociadoMapper.MapToEstadoAsociadoEntity(reader);
                //var estadoAsociado = new EstadoAsociadoEntity
                //{
                //    Id = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                //    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                //    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                //    Predeterminado = reader.GetBoolean(reader.GetOrdinal("Predeterminado"))
                //};

                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar estado asociado por nombre: {Nombre}", nombre);
                throw;
            }
        }

        public async Task<EstadoAsociadoEntity?> LeerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id no asignado", nameof(id));
            

            const string query = @"
            SELECT EstadoId, Nombre, Descripcion, Predeterminado 
            FROM EstadosAsociado
            WHERE EstadoId = @EstadoAsociadoId";
            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EstadoAsociadoId", id);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                var estadoAsociado = EstadoAsociadoMapper.MapToEstadoAsociadoEntity(reader);

                //var estadoAsociado = new EstadoAsociadoEntity
                //{
                //    EstadoAsociadoId = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                //    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                //    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                //    Predeterminado = reader.GetBoolean(reader.GetOrdinal("Predeterminado"))
                //};

                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar estado asociado por Id: {EstadoId}", id);
                throw;
            }
        }

        public async Task<EstadoAsociadoEntity?> LeerPredeterminado()
        {
            const string query = @"
            SELECT EstadoId, Nombre, Descripcion, Predeterminado 
            FROM EstadosAsociado
            WHERE Predeterminado=1";
            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();
                await using var command = new SqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                var estadoAsociado = EstadoAsociadoMapper.MapToEstadoAsociadoEntity(reader);

                //var estadoAsociado = new EstadoAsociadoEntity
                //{
                //    EstadoAsociadoId = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                //    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                //    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                //    Predeterminado = reader.GetBoolean(reader.GetOrdinal("Predeterminado"))
                //};

                return estadoAsociado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al buscar estado asociado predeterminado");
                throw;
            }
        }

        public async Task<bool> EstablecerPredeterminado(int id)
        {
            if (id<=0)
                throw new ArgumentException("Id no asignado");


            const string queryPrevia = @"
            UPDATE [dbo].[EstadosAsociado]
               SET [Predeterminado] =0";

            const string query = @"
            UPDATE [dbo].[EstadosAsociado]
               SET [Predeterminado] =1
             WHERE estadoid=@EstadoId";

            try
            {
                await using var connection = await _connectionFactory.CrearConexion();
                await connection.OpenAsync();

                await using var commandPrevia = new SqlCommand(queryPrevia, connection);
                await commandPrevia.ExecuteNonQueryAsync();

                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EstadoId", id);

                var result = await command.ExecuteNonQueryAsync();

                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al establecer estado asociado como predeterminado");
                throw;
            }
        }
    }
}
