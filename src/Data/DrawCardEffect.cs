using System.Text.Json;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class DrawCardEffect : ICardEffect
{
    public const string Id = "TEST-E-006";
    
    public int Count { get; }

    public DrawCardEffect(string initJson)
    {
        var init = JsonSerializer.Deserialize<Init>(initJson)!;

        Count = init.Count;
    }

    public string GetText() => $"Draw {Count} cards";
    
    public Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState)
    {
        playerGameState.Draw(Count);
        playerGameState.Nodes.EnemyCardTempArea.TmpShowCard(playerGameState.EnemyPeerId, card.AsDto());

        return Task.CompletedTask;
    }

    private record Init(int Count);

    public static CardEffectRecord MakeRecord(int count) => new(Id, JsonSerializer.Serialize(new Init(count)));
    
}