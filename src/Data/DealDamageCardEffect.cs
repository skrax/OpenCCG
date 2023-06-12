using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Data;

public class DealDamageCardEffect : ICardEffect
{
    private record Init(int damage);

    public static CardEffectRecord MakeRecord(int damage) => new(Id, JsonSerializer.Serialize(new Init(damage)));

    public const string Id = "TEST-E-001";

    public string GetText() => $"Deal {Damage} damage";

    public int Damage { get; }

    public DealDamageCardEffect(string initJson)
    {
        var init = JsonSerializer.Deserialize<Init>(initJson);

        Damage = init?.damage ?? throw new ArgumentNullException();
    }

    public async Task Execute(CardGameState card, PlayerGameState playerGameState)
    {
        var input = new RequireTargetInputDto(card.Record.ImgPath, RequireTargetType.All, RequireTargetSide.Enemy);
        var output = await playerGameState.Nodes.CardTempArea.RequireTargetsAsync(playerGameState.PeerId, input);

        if (output.cardId == null)
        {
            playerGameState.Enemy.Health -= 1;

            playerGameState.Nodes.EnemyStatusPanel.SetHealth(playerGameState.PeerId, playerGameState.Enemy.Health);
            playerGameState.Nodes.StatusPanel.SetHealth(playerGameState.EnemyPeerId, playerGameState.Enemy.Health);
        }
        else
        {
            var targetCard = playerGameState.Enemy.Board.Single(x => x.Id == output.cardId);
            playerGameState.ResolveDamage(targetCard, 1, PlayerGameState.ControllingEntity.Enemy);
        }
    }
}