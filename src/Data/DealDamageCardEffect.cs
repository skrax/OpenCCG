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
                sb.Append(" a friendly");
                break;
            case RequireTargetSide.Enemy:
                sb.Append(" an enemy");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (TargetType)
        {
            case RequireTargetType.All:
                break;
            case RequireTargetType.Creature:
                sb.Append(" creature");
                break;
            case RequireTargetType.Avatar:
                sb.Append(" avatar");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }

    public async Task Execute(CardGameState card, PlayerGameState playerGameState)
    {
        var cardDto = card.AsDto();
        playerGameState.Nodes.MidPanel.EndTurnButtonSetActive(playerGameState.PeerId,
            new EndTurnButtonSetActiveDto(false, "Select a Target"));
        var input = new RequireTargetInputDto(cardDto, RequireTargetType.All, RequireTargetSide.Enemy);
        var output = await playerGameState.Nodes.CardTempArea.RequireTargetsAsync(playerGameState.PeerId, input);
        playerGameState.Nodes.MidPanel.EndTurnButtonSetActive(playerGameState.PeerId,
            new EndTurnButtonSetActiveDto(true, null));
        playerGameState.Nodes.EnemyCardTempArea.TmpShowCard(playerGameState.EnemyPeerId, cardDto);

        if (output.cardId == null)
        {
            playerGameState.Enemy.Health -= Damage;

            playerGameState.Nodes.EnemyStatusPanel.SetHealth(playerGameState.PeerId, playerGameState.Enemy.Health);
            playerGameState.Nodes.StatusPanel.SetHealth(playerGameState.EnemyPeerId, playerGameState.Enemy.Health);
        }
        else
        {
            var targetCard = playerGameState.Enemy.Board.Single(x => x.Id == output.cardId);
            playerGameState.ResolveDamage(targetCard, Damage, PlayerGameState.ControllingEntity.Enemy);
        }
    }

    public static CardEffectRecord MakeRecord(int damage, RequireTargetSide side, RequireTargetType type)
    {
        return new CardEffectRecord(Id, JsonSerializer.Serialize(new Init(damage, side, type)));
    }

    private record Init(int Damage, RequireTargetSide Side, RequireTargetType Type);
}