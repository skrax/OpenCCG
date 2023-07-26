using OpenCCG.Net.Gameplay;
using OpenCCG.Net.Gameplay.Dto;
using Serilog.Core;
using Serilog.Events;

namespace OpenCCG.Core.Serilog;

public class MatchInfoEnricher : ILogEventEnricher
{
    private readonly MatchInfoDto _matchInfoDto;

    public MatchInfoEnricher(MatchInfoDto matchInfoDto)
    {
        _matchInfoDto = matchInfoDto;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var logEventProperty in _matchInfoDto.AsLogProperties(propertyFactory))
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}