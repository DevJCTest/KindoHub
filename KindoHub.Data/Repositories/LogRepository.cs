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
    public class LogRepository : ILogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<CursoRepository> _logger;

        public LogRepository(IDbConnectionFactoryFactory factory, ILogger<CursoRepository> logger)
        {
            _connectionFactory = factory.Crear("LogConnection");
            _logger = logger;
        }

        public async Task<IEnumerable<LogEntity>> LeerFiltrado(FilterLogOptions[] filters)
        {
            var queryBuilder = new StringBuilder(@"
            SELECT 
                Id, Message, MessageTemplate, Level, TimeStamp, Exception, 
                LogEvent, UserId, Username, IpAddress, RequestPath, 
                MachineName, EnvironmentName, ThreadId, SourceContext
            FROM Logs");

            var parameters = new List<SqlParameter>();
            var conditions = new List<string>();

            // Agregar condiciones para cada filtro en el array
            for (int i = 0; i < filters.Length; i++)
            {
                var filter = filters[i];
                string paramName = $"@Value{i}";
                conditions.Add(GetCondition(filter.Field, paramName));
                parameters.Add(new SqlParameter(paramName, GetParameterValue(filter.Field, filter.Value)));
            }

            if (conditions.Any())
            {
                queryBuilder.Append(" WHERE ");
                queryBuilder.Append(string.Join(" AND ", conditions));
            }

            queryBuilder.Append(" ORDER BY TimeStamp DESC");

            var logs = new List<LogEntity>();

            await using var connection = await _connectionFactory.CrearConexion();
            await connection.OpenAsync();
            await using var command = new SqlCommand(queryBuilder.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(LogMapper.MapToEntity(reader));
            }

            return logs;
        }

        public async Task<IEnumerable<LogEntity>> LeerTodos()
        {
            const string query = @"
            SELECT 
                Id, Message, MessageTemplate, Level, TimeStamp, Exception, 
                LogEvent, UserId, Username, IpAddress, RequestPath, 
                MachineName, EnvironmentName, ThreadId, SourceContext
            FROM Logs
            ORDER BY TimeStamp DESC";

            var logs = new List<LogEntity>();

            await using var connection = await _connectionFactory.CrearConexion();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(LogMapper.MapToEntity(reader));
            }

            return logs;
        }

        private string GetCondition(LogField field, string paramName)
        {
            return field switch
            {
                LogField.Id => $"Id = {paramName}",
                LogField.Message => $"Message LIKE {paramName}",
                LogField.MessageTemplate => $"MessageTemplate LIKE {paramName}",
                LogField.Level => $"Level LIKE {paramName}",
                LogField.TimeStamp => $"TimeStamp >= {paramName}",
                LogField.Exception => $"Exception LIKE {paramName}",
                LogField.LogEvent => $"LogEvent LIKE {paramName}",
                LogField.UserId => $"UserId LIKE {paramName}",
                LogField.Username => $"Username LIKE {paramName}",
                LogField.IpAddress => $"IpAddress LIKE {paramName}",
                LogField.RequestPath => $"RequestPath LIKE {paramName}",
                LogField.MachineName => $"MachineName LIKE {paramName}",
                LogField.EnvironmentName => $"EnvironmentName LIKE {paramName}",
                LogField.ThreadId => $"ThreadId = {paramName}",
                LogField.SourceContext => $"SourceContext LIKE {paramName}",
                _ => throw new ArgumentException("Campo no válido")
            };
        }

        private object GetParameterValue(LogField field, string value)
        {
            return field switch
            {
                LogField.Id => int.Parse(value),
                LogField.Message => $"%{value}%",
                LogField.MessageTemplate => $"%{value}%",
                LogField.Level => $"%{value}%",
                LogField.TimeStamp => DateTime.Parse(value),
                LogField.Exception => $"%{value}%",
                LogField.LogEvent => $"%{value}%",
                LogField.UserId => $"%{value}%",
                LogField.Username => $"%{value}%",
                LogField.IpAddress => $"%{value}%",
                LogField.RequestPath => $"%{value}%",
                LogField.MachineName => $"%{value}%",
                LogField.EnvironmentName => $"%{value}%",
                LogField.ThreadId => int.Parse(value),
                LogField.SourceContext => $"%{value}%",
                _ => throw new ArgumentException("Campo no válido")
            };
        }

    }
}
