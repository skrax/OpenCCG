using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class SquishTheWimpyCardEffect : ICardEffect
{
    public const string Id = "TEST-E-003";

    public string GetText() => "Destroy the lowest attack creature(s)";

    public async Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState)
    {
        var selfHasCreatures = playerGameState.Board.Any();
        var enemyHasCreatures = playerGameState.Enemy.Board.Any();

        if (!selfHasCreatures && !enemyHasCreatures)
        {
            // pass
        }

        else if (!selfHasCreatures)
        {
            await DestroyLowestEnemyCreaturesAsync(playerGameState);
        }

        else if (!enemyHasCreatures)
        {
            await DestroyLowestSelfCreaturesAsync(playerGameState);
        }

        else
        {
            await DestroyLowestAllCreaturesAsync(playerGameState);
        }

        playerGameState.Enemy.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, card.AsDto());
    }

    private static async Task DestroyLowestAllCreaturesAsync(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderBy(x => x.Atk).ToArray();
        var enemyByAtk = playerGameState.Enemy.Board.OrderBy(x => x.Atk).ToArray();

        var lowest = Math.Min(selfByAtk.First().Atk, enemyByAtk.First().Atk);

        var selfToDestroy = selfByAtk.TakeWhile(x => x.Atk == lowest).ToArray();

        foreach (var cardGameState in selfToDestroy)
        {
            playerGameState.DestroySelfCreature(cardGameState);
        }

        var enemyToDestroy = enemyByAtk.TakeWhile(x => x.Atk == lowest).ToArray();

        foreach (var cardGameState in enemyToDestroy)
        {
            playerGameState.DestroyEnemyCreature(cardGameState);
        }
        
        foreach (var cardGameState in selfToDestroy)
        {
            await cardGameState.OnExitAsync(playerGameState);
        }

        foreach (var cardGameState in enemyToDestroy)
        {
           await cardGameState.OnExitAsync(playerGameState.Enemy);
        }
    }


    private static async Task DestroyLowestSelfCreaturesAsync(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderBy(x => x.Atk).ToArray();
        var selfLowest = selfByAtk.First().Atk;

        var toDestroy = selfByAtk.TakeWhile(x => x.Atk == selfLowest).ToArray();
        foreach (var cardGameState in toDestroy)
        {
            playerGameState.DestroySelfCreature(cardGameState);
        }
        
        foreach (var cardGameState in toDestroy)
        {
            await cardGameState.OnExitAsync(playerGameState);
        }
    }

    private static async Task DestroyLowestEnemyCreaturesAsync(PlayerGameState playerGameState)
    {
        var enemyByAtk = playerGameState.Enemy.Board.OrderBy(x => x.Atk).ToArray();
        var enemyLowest = enemyByAtk.First().Atk;

        var toDestroy = enemyByAtk.TakeWhile(x => x.Atk == enemyLowest).ToArray();
        foreach (var cardGameState in toDestroy)
        {
            playerGameState.DestroyEnemyCreature(cardGameState);
        }
        
        foreach (var cardGameState in toDestroy)
        {
           await cardGameState.OnExitAsync(playerGameState.Enemy); 
        }
    }
}