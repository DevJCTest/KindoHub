using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    public class LogMapper
    {
        public static LogDto MapToDto(LogEntity elemento)
        {
            return new LogDto
            {
                Id = elemento.Id,
                Message = elemento.Message,
                Level = elemento.Level,
                TimeStamp = elemento.TimeStamp,
                Username = elemento.Username,
                IpAddress = elemento.IpAddress,
                RequestPath = elemento.RequestPath,
                MachineName = elemento.MachineName,
                EnvironmentName = elemento.EnvironmentName
            };
        }

    }
}
