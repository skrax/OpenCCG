using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards;

public abstract class CreatureImplementation : CardImplementation
{
    protected CreatureImplementation(
        ICreatureOutline outline,
        CreatureAbilities abilities,
        PlayerGameState2 playerGameState
    ) :
        base(outline, playerGameState)
    {
        Outline = outline;
        Abilities = abilities;
        Atk = outline.Atk;
        Def = outline.Def;
        AttacksAvailable = 0;
        MaxAttacksPerTurn = 1;
        IsExposed = Abilities.Exposed;
    }

    public new ICreatureOutline Outline { get; }

    public CreatureAbilities Abilities { get; }

    public int Atk { get; set; }

    public int Def { get; set; }

    public int AttacksAvailable { get; set; }

    public int MaxAttacksPerTurn { get; set; }

    public bool IsExposed { get; set; }

    public virtual Task OnEnterAsync() => Task.CompletedTask;
    public virtual Task OnExitAsync() => Task.CompletedTask;
    public virtual Task OnStartTurnAsync() => Task.CompletedTask;
    public virtual Task OnEndTurnAsync() => Task.CompletedTask;
    public virtual Task OnStartCombatTurnAsync() => Task.CompletedTask;
    public virtual Task OnEndCombatAsync() => Task.CompletedTask;

    public async Task OnUpkeepAsync()
    {
        IsExposed = true;
        AttacksAvailable = MaxAttacksPerTurn;

        await UpdateAsync();
    }

    public async Task OnEnterBoardAsync()
    {
        IsExposed = Abilities.Exposed || Abilities.Defender;
        AttacksAvailable = Abilities.Haste ? MaxAttacksPerTurn : 0;

        var dto = AsDto();

        PlayerGameState.Nodes.Hand.RemoveCard(PlayerGameState.PeerId, Id);
        PlayerGameState.Enemy.Nodes.EnemyHand.RemoveCard(PlayerGameState.EnemyPeerId);

        await Task.WhenAll(
            PlayerGameState.Nodes.Board.PlaceCard(PlayerGameState.PeerId, dto),
            PlayerGameState.Enemy.Nodes.EnemyBoard.PlaceCard(PlayerGameState.EnemyPeerId, dto));
    }

    public async Task OnEndStepAsync()
    {
        AttacksAvailable = 0;
        await UpdateAsync();
    }

    private async Task UpdateAsync()
    {
        var dto = AsDto();
        var t1 = PlayerGameState.Nodes.Board.UpdateCardAsync(PlayerGameState.PeerId, dto);
        var t2 = PlayerGameState.Nodes.EnemyBoard.UpdateCardAsync(PlayerGameState.EnemyPeerId, dto);

        await Task.WhenAll(t1, t2);
    }
}