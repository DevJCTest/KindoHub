using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public enum LogField
    {
        Id,
        Message,
        MessageTemplate,
        Level,
        TimeStamp,
        Exception,
        LogEvent,
        UserId,
        Username,
        IpAddress,
        RequestPath,
        MachineName,
        EnvironmentName,
        ThreadId,
        SourceContext
    }
}
