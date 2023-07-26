using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace OpenCCG.Net.Gameplay.Dto;

public record MatchInfoDto
(
    Guid SessionId,
    Guid PlayerId,
    Guid EnemyPlayerId
)
{
    public IEnumerable<LogEventProperty> AsLogProperties(ILogEventPropertyFactory propertyFactory)
    {
        return new[]
        {
            propertyFactory.CreateProperty("SessionId", SessionId),
            propertyFactory.CreateProperty("PlayerId", PlayerId),
            propertyFactory.CreateProperty("EnemyPlayerId", EnemyPlayerId)
        };
    }


    public static string[] LogPropertyNames()
    {
        return new[]
        {
            "SessionId",
            "PlayerId",
            "EnemyPlayerId"
        };
    }
}