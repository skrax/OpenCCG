using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Data;

public class DealDamageCardEffect : ICardEffect
{
    public const string Id = "TEST-E-001";

    public DealDamageCardEffect(string initJson)
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
                break;
            case RequireTargetSide.Friendly:
                sb.Append(" to a friendly");
                break;
            case RequireTargetSide.Enemy:
                sb.Append(" to an enemy");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (TargetType)
        {
            case RequireTargetType.All:
                break;
            case RequireTargetType.Creature:
                sb.Append(TargetSide == RequireTargetSide.All ? " to a creature" : " creature");
                break;
            case RequireTargetType.Avatar:
                sb.Append(TargetSide == RequireTargetSide.All ? " to an avatar" : " avatar");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }

    public async Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState)
    {
        var cardDto = card.AsDto();
        playerGameState.Nodes.MidPanel.EndTurnButtonSetActive(playerGameState.PeerId,
            new EndTurnButtonSetActiveDto(false, "Select a Target"));
        var input = new RequireTargetInputDto(cardDto, RequireTargetType.All, RequireTargetSide.Enemy);
        var output = await playerGameState.Nodes.CardEffectPreview.RequireTargetsAsync(playerGameState.PeerId, input);
        playerGameState.Nodes.MidPanel.EndTurnButtonSetActive(playerGameState.PeerId,
            new EndTurnButtonSetActiveDto(playerGameState.IsTurn, null));
        playerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, cardDto);

        // check if targeting was skipped
        if (output.cardId == null && output.isEnemyAvatar == null) return;

        if (output.cardId == null)
        {
            if (output.isEnemyAvatar!.Value)
            {
                playerGameState.Enemy.Health -= Damage;

                playerGameState.Nodes.EnemyStatusPanel.SetHealth(playerGameState.PeerId,
                    playerGameState.Enemy.Health);
                playerGameState.Nodes.StatusPanel.SetHealth(playerGameState.EnemyPeerId,
                    playerGameState.Enemy.Health);
            }
            else if (!output.isEnemyAvatar.Value)
            {
                playerGameState.Health -= Damage;

                playerGameState.Nodes.EnemyStatusPanel.SetHealth(playerGameState.EnemyPeerId,
                    playerGameState.Health);
                playerGameState.Nodes.StatusPanel.SetHealth(playerGameState.PeerId, playerGameState.Health);
            }
        }
        else
        {
            if (TargetSide == RequireTargetSide.Friendly)
            {
                var targetCard = playerGameState.Board.Single(x => x.Id == output.cardId);
                await playerGameState.ResolveDamageAsync(targetCard, Damage);
                if (targetCard.Zone == CardZone.Pit)
                    await targetCard.OnExitAsync(playerGameState);
            }
            else if (TargetSide == RequireTargetSide.Enemy)
            {
                var targetCard = playerGameState.Enemy.Board.Single(x => x.Id == output.cardId);
                await playerGameState.ResolveDamageAsync(targetCard, Damage);

                if (targetCard.Zone == CardZone.Pit)
                    await targetCard.OnExitAsync(playerGameState.Enemy);
            }
            else
            {
                var targetCard = playerGameState.Board.SingleOrDefault(x => x.Id == output.cardId);
                if (targetCard != null)
                {
                    await playerGameState.ResolveDamageAsync(targetCard, Damage);
                    if (targetCard.Zone == CardZone.Pit)
                        await targetCard.OnExitAsync(playerGameState);
                }

                targetCard = playerGameState.Enemy.Board.Single(x => x.Id == output.cardId);
                await playerGameState.ResolveDamageAsync(targetCard, Damage);

                if (targetCard.Zone == CardZone.Pit)
                    await targetCard.OnExitAsync(playerGameState.Enemy);
            }
        }
    }

    public static CardEffectRecord MakeRecord(int damage, RequireTargetSide side, RequireTargetType type)
    {
        return new CardEffectRecord(Id, JsonSerializer.Serialize(new Init(damage, side, type)));
    }

    private record Init(int Damage, RequireTargetSide Side, RequireTargetType Type);
}