using System;
using OpenCCG.Data;
using OpenCCG.Net.Dto;

namespace OpenCCG.Net;

public class CardGameState
{
    public readonly Guid Id = Guid.NewGuid();
    public CardRecord Record { get; init; }

    public CardZone Zone { get; set; }

    public int Atk { get; set; }
    public int Def { get; set; }
    public int Cost { get; set; }

    public int AttacksAvailable { get; set; }

    public int MaxAttacksPerTurn { get; set; }

    public bool IsSummoningSicknessOn { get; set; }

    public bool IsSummoningProtectionOn { get; set; }

    public void ResetStats()
    {
        Atk = Record.Atk;
        Def = Record.Def;
        Cost = Record.Cost;
    }

    public CardGameStateDto AsDto()
    {
        var recordMod = new CardRecordMod(
            Atk - Record.Atk,
            Def - Record.Def,
            Cost - Record.Cost
        );

        return new CardGameStateDto(
            Id,
            Record,
            recordMod,
            Zone,
            IsSummoningSicknessOn,
            IsSummoningProtectionOn,
            AttacksAvailable
        );
    }
}