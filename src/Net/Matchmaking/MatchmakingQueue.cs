using System.Collections.Generic;
using OpenCCG.Cards;
using OpenCCG.Cards.Test;

namespace OpenCCG.Net.Matchmaking;

public class MatchmakingQueue
{
    private readonly Queue<QueuedPlayer> _queue = new();

    public void Enqueue(long peerId, MatchmakingRequest dto)
    {
        var deckList = new List<ICardOutline>(30);
        foreach (var card in dto.Deck.list)
        {
            var cardRecord = TestSetOutlines.Cards[card.Id];
            for (var i = 0; i < card.Count; ++i) deckList.Add(cardRecord);
        }

        var entry = new QueuedPlayer(peerId, deckList);

        _queue.Enqueue(entry);
    }

    public bool TryGetPair(out QueuedPlayer player1, out QueuedPlayer player2)
    {
        if (_queue.Count < 2)
        {
            player1 = null!;
            player2 = null!;
            
            return false;
        }

        player1 = _queue.Dequeue();
        player2 = _queue.Dequeue();

        return true;
    }
}