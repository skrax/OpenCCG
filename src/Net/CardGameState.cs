using System;
using System.Linq;
using System.Threading.Tasks;
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


    public async Task OnSpellAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.Spell, playerGameState);

    public async Task OnEnterAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.Enter, playerGameState);

    public async Task OnExitAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.Exit, playerGameState);

    public async Task OnStartTurnAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.StartTurn, playerGameState);

    public async Task OnEndTurnAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.EndTurn, playerGameState);

    public async Task OnStartCombatAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.StartCombat, playerGameState);

    public async Task OnEndCombatAsync(PlayerGameState playerGameState) =>
        await ExecuteEffectAsync(Record.CardEffects.EndCombat, playerGameState);


    private async Task ExecuteEffectAsync(CardEffectRecord? effect, PlayerGameState playerGameState)
    {
        if (effect == null) return;

        await Database.CardEffects[effect.Id](effect.InitJson).ExecuteAsync(this, playerGameState);
    }
}