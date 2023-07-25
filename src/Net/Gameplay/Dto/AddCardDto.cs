using System;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Dto;

public record AddCardDto
(
    Guid PlayerId,
    CardImplementationDto? Dto
);