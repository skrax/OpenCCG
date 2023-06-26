using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Data;

public class AoeDamageCardEffect : ICardEffect
{
    public const string Id = "TEST-E-004";

    public AoeDamageCardEffect(string initJson)
    {
        var init = JsonSerializer.Deserialize<Init>(initJson);

        Damage = init?.Damage ?? throw new ArgumentNullException();
        TargetSide = init.Side;
        TargetType = init.Type;
    }

    public int Damage { get; }

    public RequireTargetSide TargetSide { get; }

    public RequireTargetType TargetType { get; }

    public string GetText()
    {
        var sb = new StringBuilder();
        sb.Append($"Deal {Damage} damage");

        switch (TargetSide)
        {
            case RequireTargetSide.All:
                sb.Append(" to all");
                break;
            case RequireTargetSide.Friendly:
                sb.Append(" to all friendly");
                break;
            case RequireTargetSide.Enemy:
                sb.Append(" to all enemy");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (TargetType)
        {
            case RequireTargetType.All:
                sb.Append(" creatures and avatars");
                break;
            case RequireTargetType.Creature:
                sb.Append(" creatures");
                break;
            case RequireTargetType.Avatar:
                sb.Append(" avatars");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }

    public async Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState)
    {
        var cardDto = card.AsDto();
        playerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, cardDto);

        var killedSelfCreatures = new List<CardGameState>();
        var killedEnemyCreatures = new List<CardGameState>();

        if (TargetType == RequireTargetType.Creature)
        {
            if (TargetSide is RequireTargetSide.All or RequireTargetSide.Friendly)
            {
                var board = playerGameState.Board.ToArray();

                await Task.WhenAll(board.Select(async x =>
                {
                    await playerGameState.ResolveDamageAsync(x, Damage);
                    if (x.Zone == CardZone.Pit)
                        killedSelfCreatures.Add(x);
                }));
            }

            if (TargetSide is RequireTargetSide.All or RequireTargetSide.Enemy)
            {
                var board = playerGameState.Enemy.Board.ToArray();

                await Task.WhenAll(board.Select(async x =>
                {
                    await playerGameState.ResolveDamageAsync(x, Damage);
                    if (x.Zone == CardZone.Pit)
                        killedEnemyCreatures.Add(x);
                }));
            }
        }

        if (TargetType == RequireTargetType.Avatar)
        {
            if (TargetSide is RequireTargetSide.All or RequireTargetSide.Friendly)
            {
                playerGameState.Health -= Damage;

                playerGameState.Nodes.StatusPanel.SetHealth(playerGameState.PeerId, playerGameState.Enemy.Health);
                playerGameState.Nodes.EnemyStatusPanel.SetHealth(playerGameState.EnemyPeerId,
                    playerGameState.Enemy.Health);
            }

            if (TargetSide is RequireTargetSide.All or RequireTargetSide.Enemy)
            {
                playerGameState.Enemy.Health -= Damage;

                playerGameState.Nodes.EnemyStatusPanel.SetHealth(playerGameState.PeerId, playerGameState.Enemy.Health);
                playerGameState.Nodes.StatusPanel.SetHealth(playerGameState.EnemyPeerId, playerGameState.Enemy.Health);
            }
        }

        foreach (var cardGameState in killedSelfCreatures)
        {
            await cardGameState.OnExitAsync(playerGameState);
        }

        foreach (var killedEnemyCreature in killedEnemyCreatures)
        {
            await killedEnemyCreature.OnExitAsync(playerGameState.Enemy);
        }
    }

    public static CardEffectRecord MakeRecord(int damage, RequireTargetSide side, RequireTargetType type)
    {
        return new CardEffectRecord(Id, JsonSerializer.Serialize(new Init(damage, side, type)));
    }

    private record Init(int Damage, RequireTargetSide Side, RequireTargetType Type);
}