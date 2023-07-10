using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class SquishTheWimpy : SpellImplementation
{
    public SquishTheWimpy(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        var board = PlayerGameState.Board;
        var enemyBoard = PlayerGameState.Enemy.Board;

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());

        if (board.Any() && enemyBoard.Any())
            await ResolveAllBoardsAsync();
        else if (board.Any())
            await ResolveBoardAsync();
        else if (enemyBoard.Any()) await ResolveEnemyBoardAsync();
    }

    private async Task ResolveAllBoardsAsync()
    {
        var ordered = PlayerGameState
                      .Board
                      .Cast<CreatureImplementation>()
                      .OrderBy(x => x.CreatureState.Atk)
                      .ToList();

        var orderedEnemy = PlayerGameState
                           .Enemy.Board
                           .Cast<CreatureImplementation>()
                           .OrderBy(x => x.CreatureState.Atk)
                           .ToList();

        var atk = Math.Max(ordered.First().CreatureState.Atk, orderedEnemy.First().CreatureState.Atk);

        var creatures = ordered
                        .TakeWhile(x => x.CreatureState.Atk == atk)
                        .Concat(orderedEnemy.TakeWhile(x => x.CreatureState.Atk == atk))
                        .ToList();

        foreach (var creature in creatures) creature.MoveToZone(CardZone.None);

        await Task.WhenAll(creatures.Select(x => x.RemoveFromBoardAsync()));

        foreach (var creature in creatures)
        {
            await creature.OnExitAsync();
            creature.MoveToZone(CardZone.Pit);
        }
    }

    private async Task ResolveBoardAsync()
    {
        var ordered = PlayerGameState
                      .Board
                      .Cast<CreatureImplementation>()
                      .OrderBy(x => x.CreatureState.Atk)
                      .ToList();

        var atk = ordered.First().CreatureState.Atk;

        var creatures = ordered
                        .TakeWhile(x => x.CreatureState.Atk == atk)
                        .ToList();

        foreach (var creature in creatures) creature.MoveToZone(CardZone.None);

        await Task.WhenAll(creatures.Select(x => x.RemoveFromBoardAsync()));

        foreach (var creature in creatures)
        {
            await creature.OnExitAsync();
            creature.MoveToZone(CardZone.Pit);
        }
    }

    private async Task ResolveEnemyBoardAsync()
    {
        var ordered = PlayerGameState
                      .Enemy.Board
                      .Cast<CreatureImplementation>()
                      .OrderBy(x => x.CreatureState.Atk)
                      .ToList();

        var atk = ordered.First().CreatureState.Atk;

        var creatures = ordered
                        .TakeWhile(x => x.CreatureState.Atk == atk)
                        .ToList();

        foreach (var creature in creatures) creature.MoveToZone(CardZone.None);

        await Task.WhenAll(creatures.Select(x => x.RemoveFromBoardAsync()));

        foreach (var creature in creatures)
        {
            await creature.OnExitAsync();
            creature.MoveToZone(CardZone.Pit);
        }
    }
}