using System;

namespace OpenCCG.Net.Gameplay.Dto;

public record RemoveCardDto
(
    Guid PlayerId,
    Guid? CardId
);