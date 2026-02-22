using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Transformers
{
    public class LogMapper
    {
        public static LogEntity MapToEntity(SqlDataReader reader)
        {
            return new LogEntity
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Message = reader.IsDBNull(reader.GetOrdinal("Message")) ? null : reader.GetString(reader.GetOrdinal("Message")),
                MessageTemplate = reader.IsDBNull(reader.GetOrdinal("MessageTemplate")) ? null : reader.GetString(reader.GetOrdinal("MessageTemplate")),
                Level = reader.IsDBNull(reader.GetOrdinal("Level")) ? null : reader.GetString(reader.GetOrdinal("Level")),
                TimeStamp = reader.IsDBNull(reader.GetOrdinal("TimeStamp")) ? null : reader.GetDateTime(reader.GetOrdinal("TimeStamp")),
                Exception = reader.IsDBNull(reader.GetOrdinal("Exception")) ? null : reader.GetString(reader.GetOrdinal("Exception")),
                LogEvent = reader.IsDBNull(reader.GetOrdinal("LogEvent")) ? null : reader.GetString(reader.GetOrdinal("LogEvent")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
                Username = reader.IsDBNull(reader.GetOrdinal("Username")) ? null : reader.GetString(reader.GetOrdinal("Username")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString(reader.GetOrdinal("IpAddress")),
                RequestPath = reader.IsDBNull(reader.GetOrdinal("RequestPath")) ? null : reader.GetString(reader.GetOrdinal("RequestPath")),
                MachineName = reader.IsDBNull(reader.GetOrdinal("MachineName")) ? null : reader.GetString(reader.GetOrdinal("MachineName")),
                EnvironmentName = reader.IsDBNull(reader.GetOrdinal("EnvironmentName")) ? null : reader.GetString(reader.GetOrdinal("EnvironmentName")),
                ThreadId = reader.IsDBNull(reader.GetOrdinal("ThreadId")) ? null : reader.GetInt32(reader.GetOrdinal("ThreadId")),
                SourceContext = reader.IsDBNull(reader.GetOrdinal("SourceContext")) ? null : reader.GetString(reader.GetOrdinal("SourceContext"))
            };
        }
    }
}
