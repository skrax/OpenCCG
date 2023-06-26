using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class FellTheMightyCardEffect : ICardEffect
{
    public const string Id = "TEST-E-002";

    public string GetText() => "Destroy the highest attack creature(s)";

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
            await DestroyHighestEnemyCreaturesAsync(playerGameState);
        }

        else if (!enemyHasCreatures)
        {
            await DestroyHighestSelfCreaturesAsync(playerGameState);
        }

        else
        {
            await DestroyHighestAllCreaturesAsync(playerGameState);
        }
        
        playerGameState.Enemy.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, card.AsDto());
    }

    private static async Task DestroyHighestAllCreaturesAsync(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderByDescending(x => x.Atk).ToArray();
        var enemyByAtk = playerGameState.Enemy.Board.OrderByDescending(x => x.Atk).ToArray();

        var highest = Math.Max(selfByAtk.First().Atk, enemyByAtk.First().Atk);

        var toDestroySelf = selfByAtk.TakeWhile(x => x.Atk == highest).ToArray();
        var toDestroyEnemy = enemyByAtk.TakeWhile(x => x.Atk == highest).ToArray();

        foreach (var cardGameState in toDestroySelf)
        {
            cardGameState.DestroyCreature();
        }

        foreach (var cardGameState in toDestroyEnemy)
        {
            cardGameState.DestroyCreature();
        }
        
        foreach (var cardGameState in toDestroySelf)
        {
            await cardGameState.OnExitAsync(playerGameState);
        }

        foreach (var cardGameState in toDestroyEnemy)
        {
            await cardGameState.OnExitAsync(playerGameState.Enemy);
        }
    }


    private static async Task DestroyHighestSelfCreaturesAsync(PlayerGameState playerGameState)
    {
        var selfByAtk = playerGameState.Board.OrderByDescending(x => x.Atk).ToArray();
        var selfHighest = selfByAtk.First().Atk;

        var toDestroy = selfByAtk.TakeWhile(x => x.Atk == selfHighest).ToArray();
        foreach (var cardGameState in toDestroy)
        {
            cardGameState.DestroyCreature();
        }
        
        foreach (var cardGameState in toDestroy)
        {
           await cardGameState.OnExitAsync(playerGameState); 
        }
    }

    private static async Task DestroyHighestEnemyCreaturesAsync(PlayerGameState playerGameState)
    {
        var enemyByAtk = playerGameState.Enemy.Board.OrderByDescending(x => x.Atk).ToArray();
        var enemyHighest = enemyByAtk.First().Atk;

        var toDestroy = enemyByAtk.TakeWhile(x => x.Atk == enemyHighest).ToArray();
        foreach (var cardGameState in toDestroy)
        {
            cardGameState.DestroyCreature();
        }
        
        foreach (var cardGameState in toDestroy)
        {
           await cardGameState.OnExitAsync(playerGameState.Enemy); 
        }
    }
}