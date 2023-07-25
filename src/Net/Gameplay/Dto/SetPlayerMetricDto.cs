using System;

namespace OpenCCG.Net.Gameplay.Dto;

public record SetPlayerMetricDto
(
    Guid PlayerId,
    int Value 
);