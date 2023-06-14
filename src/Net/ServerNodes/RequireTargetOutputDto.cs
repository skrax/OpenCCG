using System;

namespace OpenCCG.Net.ServerNodes;

public record RequireTargetOutputDto(Guid? cardId, bool? isEnemyAvatar);