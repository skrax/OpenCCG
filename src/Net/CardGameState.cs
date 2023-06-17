using System;
using System.Linq;
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


    public void OnSpell(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.Spell, playerGameState);

    public void OnEnter(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.Enter, playerGameState);

    public void OnExit(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.Exit, playerGameState);

    public void OnStartTurn(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.StartTurn, playerGameState);

    public void OnEndTurn(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.EndTurn, playerGameState);

    public void OnStartCombat(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.StartCombat, playerGameState);

    public void OnEndCombat(PlayerGameState playerGameState) =>
        ExecuteEffect(Record.CardEffects.EndCombat, playerGameState);


    private void ExecuteEffect(CardEffectRecord? effect, PlayerGameState playerGameState)
    {
        if (effect == null) return;

        Database.CardEffects[effect.Id](effect.InitJson).Execute(this, playerGameState);
    }
}