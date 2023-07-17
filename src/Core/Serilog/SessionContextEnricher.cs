using Serilog.Core;
using Serilog.Events;

namespace OpenCCG.Core.Serilog;

public class SessionContextEnricher : ILogEventEnricher
{
    private readonly SessionContext _sessionContext;

    public SessionContextEnricher(SessionContext sessionContext)
    {
        _sessionContext = sessionContext;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var logEventProperty in _sessionContext.AsLogProperties(propertyFactory))
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}