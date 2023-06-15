using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class FellTheMightyCardEffect : ICardEffect
{
    public const string Id = "TEST-E-002";

    public string GetText() => "Destroy the highest attack creature(s)";

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
            DestroyHighestEnemyCreatures(playerGameState);
        }

        else if (!enemyHasCreatures)
        {
            DestroyHighestSelfCreatures(playerGameState);
        }

        else
        {
            DestroyHighestAllCreatures(playerGameState);
        }
        
        playerGameState.Enemy.Nodes.EnemyCardTempArea.TmpShowCard(playerGameState.EnemyPeerId, card.AsDto());

        return Task.CompletedTask;
    }

    private static void DestroyHighestAllCreatures(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderByDescending(x => x.Atk).ToArray();
        var enemyByAtk = playerGameState.Enemy.Board.OrderByDescending(x => x.Atk).ToArray();

        var highest = Math.Max(selfByAtk.First().Atk, enemyByAtk.First().Atk);

        foreach (var cardGameState in selfByAtk.TakeWhile(x => x.Atk == highest))
        {
            playerGameState.DestroySelfCreature(cardGameState);
        }

        foreach (var cardGameState in enemyByAtk.TakeWhile(x => x.Atk == highest))
        {
            playerGameState.DestroyEnemyCreature(cardGameState);
        }
    }


    private static void DestroyHighestSelfCreatures(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderByDescending(x => x.Atk).ToArray();
        var selfHighest = selfByAtk.First().Atk;

        var toDestroy = selfByAtk.TakeWhile(x => x.Atk == selfHighest);
        foreach (var cardGameState in toDestroy)
        {
            playerGameState.DestroySelfCreature(cardGameState);
        }
    }

    private static void DestroyHighestEnemyCreatures(PlayerGameState playerGameState)
    {
        var enemyByAtk = playerGameState.Enemy.Board.OrderByDescending(x => x.Atk).ToArray();
        var enemyHighest = enemyByAtk.First().Atk;

        var toDestroy = enemyByAtk.TakeWhile(x => x.Atk == enemyHighest);
        foreach (var cardGameState in toDestroy)
        {
            playerGameState.DestroyEnemyCreature(cardGameState);
        }
    }
}