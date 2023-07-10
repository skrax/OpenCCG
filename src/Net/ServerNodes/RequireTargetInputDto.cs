using OpenCCG.Cards;

namespace OpenCCG.Net.ServerNodes;

public record RequireTargetInputDto(CardImplementationDto Card, RequireTargetType Type, RequireTargetSide Side);