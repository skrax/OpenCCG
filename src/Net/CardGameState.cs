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


    public void OnEnter(PlayerGameState playerGameState) => ExecuteEffects(Record.CardEffects.Enter, playerGameState);
    public void OnExit(PlayerGameState playerGameState) => ExecuteEffects(Record.CardEffects.Exit, playerGameState);

    public void OnStartTurn(PlayerGameState playerGameState) =>
        ExecuteEffects(Record.CardEffects.StartTurn, playerGameState);

    public void OnEndTurn(PlayerGameState playerGameState) =>
        ExecuteEffects(Record.CardEffects.EndTurn, playerGameState);

    public void OnStartCombat(PlayerGameState playerGameState) =>
        ExecuteEffects(Record.CardEffects.StartCombat, playerGameState);

    public void OnEndCombat(PlayerGameState playerGameState) =>
        ExecuteEffects(Record.CardEffects.EndCombat, playerGameState);


    private void ExecuteEffects(CardEffectRecord[] effects, PlayerGameState playerGameState)
    {
        foreach (var cardEffect in effects.Select(x => Database.CardEffects[x.Id](x.InitJson)))
        {
            cardEffect.Execute(this, playerGameState);
        }
    }
}