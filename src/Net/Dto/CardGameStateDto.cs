using System;
using OpenCCG.Data;

namespace OpenCCG.Net.Dto;

public record CardGameStateDto
(
    Guid Id,
    CardRecord Record,
    CardRecordMod RecordMod,
    CardZone Zone,
    bool IsSummoningSicknessOn,
    bool ISummoningProtectionOn
)
{
    public int Atk => Record.Atk + RecordMod.Atk;
    public int Def => Record.Def + RecordMod.Def;
    public int Cost => Record.Cost + RecordMod.Cost;
}

public record CardRecordMod
(
    int Atk,
    int Def,
    int Cost
);