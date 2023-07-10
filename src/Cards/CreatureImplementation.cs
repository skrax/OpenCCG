using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards;

public abstract class CreatureImplementation : CardImplementation
{
    protected CreatureImplementation(
        CreatureOutline outline,
        CreatureAbilities abilities,
        PlayerGameState playerGameState
    ) :
        base(outline, playerGameState,
            new CreatureState
            {
                Atk = outline.Atk,
                Def = outline.Def,
                AttacksAvailable = 0,
                MaxAttacksPerTurn = 1,
                IsExposed = abilities.Exposed
            }
        )
    {
        Abilities = abilities;
    }

    public CreatureOutline CreatureOutline => (CreatureOutline)Outline;

    public CreatureState CreatureState => (CreatureState)State;

    public CreatureAbilities Abilities { get; }

    public virtual Task OnEnterAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnExitAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnStartTurnAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnEndTurnAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnStartCombatTurnAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnEndCombatAsync()
    {
        return Task.CompletedTask;
    }

    public async Task OnUpkeepAsync()
    {
        CreatureState.IsExposed = true;
        CreatureState.AttacksAvailable = CreatureState.MaxAttacksPerTurn;

        await UpdateAsync();
    }

    public async Task EnterBoardAsync()
    {
        CreatureState.IsExposed = Abilities.Exposed || Abilities.Defender;
        CreatureState.AttacksAvailable = Abilities.Haste ? CreatureState.MaxAttacksPerTurn : 0;

        var dto = AsDto();

        await PlaceOnBoard(dto);
    }

    private async Task PlaceOnBoard(CardImplementationDto dto)
    {
        await Task.WhenAll(
            PlayerGameState.Nodes.Board.PlaceCard(PlayerGameState.PeerId, dto),
            PlayerGameState.Enemy.Nodes.EnemyBoard.PlaceCard(PlayerGameState.EnemyPeerId, dto));
    }

    public async Task OnEndStepAsync()
    {
        CreatureState.AttacksAvailable = 0;
        await UpdateAsync();
    }

    public async Task UpdateAsync()
    {
        var dto = AsDto();
        var t1 = PlayerGameState.Nodes.Board.UpdateCardAsync(PlayerGameState.PeerId, dto);
        var t2 = PlayerGameState.Nodes.EnemyBoard.UpdateCardAsync(PlayerGameState.EnemyPeerId, dto);

        await Task.WhenAll(t1, t2);
    }

    public async Task RemoveFromBoardAsync()
    {
        await Task.WhenAll(PlayerGameState.Nodes.Board.RemoveCard(PlayerGameState.PeerId, Id),
            PlayerGameState.Nodes.EnemyBoard.RemoveCard(PlayerGameState.EnemyPeerId, Id));
    }

    public async Task TakeDamageAsync(int amount)
    {
        if (amount <= 0) return;
        CreatureState.Def -= amount;

        await UpdateAsync();
    }

    public override CardImplementationDto AsDto()
    {
        return CardImplementationDto.AsCreature(Id, CreatureOutline, CreatureState, Abilities);
    }
}