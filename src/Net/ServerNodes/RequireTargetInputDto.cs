using OpenCCG.Net.Dto;

namespace OpenCCG.Net.ServerNodes;

public record RequireTargetInputDto(CardGameStateDto Card, RequireTargetType Type, RequireTargetSide Side);