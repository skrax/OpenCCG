using System.Text.Json;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class DrawCardEffect : ICardEffect
{
    public const string Id = "TEST-E-006";

    public DrawCardEffect(string initJson)
    {
        var init = JsonSerializer.Deserialize<Init>(initJson)!;

        Count = init.Count;
    }

    public int Count { get; }

    public string GetText()
    {
        return $"Draw {Count} cards";
    }

    public Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState)
    {
        playerGameState.Draw(Count);
        playerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, card.AsDto());

        return Task.CompletedTask;
    }

    public static CardEffectRecord MakeRecord(int count)
    {
        return new CardEffectRecord(Id, JsonSerializer.Serialize(new Init(count)));
    }

    private record Init(int Count);
}