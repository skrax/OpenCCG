using System;
using System.Text.Json;
using OpenCCG.Net;

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

    public void Execute(CardGameState card, PlayerGameState playerGameState)
    {
        /* TODO
        var requestId = Guid.NewGuid().ToString();
        playerGameState.Nodes.CardTempArea.RpcId(playerGameState.PeerId, "RequireTargets", requestId,
            card.Record.ImgPath);
            */
    }
}