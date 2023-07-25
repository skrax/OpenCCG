using System;

namespace OpenCCG.Net.Gameplay.Dto;

public record MatchInfoDto
(
    Guid SessionId,
    Guid PlayerId,
    Guid EnemyPlayerId
);