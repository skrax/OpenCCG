using System;
using OpenCCG.Data;

namespace OpenCCG.Net.Dto;

public record CardGameStateDto
(
    Guid Id,
    CardRecord Record,
    CardZone Zone,
    CardZone OldZone
);