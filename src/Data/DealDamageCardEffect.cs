using System;
using System.Linq;
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

        Damage = init?.damage ?? throw new ArgumentNullException();
    }

    public int Damage { get; }

    public string GetText()
    {
        return $"Deal {Damage} damage";
    }

    public async Task Execute(CardGameState card, PlayerGameState playerGameState)
    {
        var input = new RequireTargetInputDto(card.Record.ImgPath, RequireTargetType.All, RequireTargetSide.Enemy);
        var output = await playerGameState.Nodes.CardTempArea.RequireTargetsAsync(playerGameState.PeerId, input);

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

    public static CardEffectRecord MakeRecord(int damage)
    {
        return new CardEffectRecord(Id, JsonSerializer.Serialize(new Init(damage)));
    }

    private record Init(int damage);
}