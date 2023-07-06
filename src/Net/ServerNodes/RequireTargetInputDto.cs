using OpenCCG.Cards;
using OpenCCG.Net.Dto;

namespace OpenCCG.Net.ServerNodes;

public record RequireTargetInputDto(CardImplementationDto Card, RequireTargetType Type, RequireTargetSide Side);