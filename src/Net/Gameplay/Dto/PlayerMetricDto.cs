using System;

namespace OpenCCG.Net.Gameplay.Dto;

public record PlayerMetricDto
(
    Guid PlayerId,
    int Value,
    int? MaxValue = null
);