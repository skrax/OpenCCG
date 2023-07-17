using System;
using Serilog.Core;
using Serilog.Events;

namespace OpenCCG.Core;

public record SessionContext(Guid SessionId)
{
    public LogEventProperty[] AsLogProperties(ILogEventPropertyFactory factory)
    {
        return new[]
        {
            factory.CreateProperty("SessionId", SessionId)
        };
    }

    public static string[] LogPropertyNames()
    {
        return new[]
        {
            "SessionId"
        };
    }
}