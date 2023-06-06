using System;

namespace OpenCCG.Net.Dto;

public record CombatPlayerCardDto(Guid AttackerId, Guid TargetId);