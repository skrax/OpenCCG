using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class SquishTheWimpyCardEffect : ICardEffect
{
    public const string Id = "TEST-E-003";

    public string GetText() => "Destroy the lowest attack creature(s)";

    public Task Execute(CardGameState card, PlayerGameState playerGameState)
    {
        var selfHasCreatures = playerGameState.Board.Any();
        var enemyHasCreatures = playerGameState.Enemy.Board.Any();

        if (!selfHasCreatures && !enemyHasCreatures)
        {
            // pass
        }


        else if (!selfHasCreatures)
        {
            DestroyLowestEnemyCreatures(playerGameState);
        }

        else if (!enemyHasCreatures)
        {
            DestroyLowestSelfCreatures(playerGameState);
        }

        else
        {
            DestroyLowestAllCreatures(playerGameState);
        }

        playerGameState.Enemy.Nodes.EnemyCardTempArea.TmpShowCard(playerGameState.EnemyPeerId, card.AsDto());

        return Task.CompletedTask;
    }

    private static void DestroyLowestAllCreatures(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderBy(x => x.Atk).ToArray();
        var enemyByAtk = playerGameState.Enemy.Board.OrderBy(x => x.Atk).ToArray();

        var lowest = Math.Min(selfByAtk.First().Atk, enemyByAtk.First().Atk);

        foreach (var cardGameState in selfByAtk.TakeWhile(x => x.Atk == lowest))
        {
            playerGameState.DestroySelfCreature(cardGameState);
        }

        foreach (var cardGameState in enemyByAtk.TakeWhile(x => x.Atk == lowest))
        {
            playerGameState.DestroyEnemyCreature(cardGameState);
        }
    }


    private static void DestroyLowestSelfCreatures(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderBy(x => x.Atk).ToArray();
        var selfLowest = selfByAtk.First().Atk;

        var toDestroy = selfByAtk.TakeWhile(x => x.Atk == selfLowest);
        foreach (var cardGameState in toDestroy)
        {
            playerGameState.DestroySelfCreature(cardGameState);
        }
    }

    private static void DestroyLowestEnemyCreatures(PlayerGameState playerGameState)
    {
        var enemyByAtk = playerGameState.Enemy.Board.OrderBy(x => x.Atk).ToArray();
        var enemyLowest = enemyByAtk.First().Atk;

        var toDestroy = enemyByAtk.TakeWhile(x => x.Atk == enemyLowest);
        foreach (var cardGameState in toDestroy)
        {
            playerGameState.DestroyEnemyCreature(cardGameState);
        }
    }
}