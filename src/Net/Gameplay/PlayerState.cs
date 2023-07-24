using System;
using System.Collections.Generic;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay;

public class PlayerState
{
    public readonly long PeerId;
    public readonly long EnemyPeerId;
    public PlayerState Enemy = null!;
    public readonly List<ICardOutline> DeckList;
    public readonly Deck Deck;
    public readonly Hand Hand;
    public readonly Board Board;
    public readonly Pit Pit;

    public PlayerState(long peerId, long enemyPeerId, List<ICardOutline> deckList)
    {
        PeerId = peerId;
        EnemyPeerId = enemyPeerId;
        DeckList = deckList;
        // TODO
        /**
        var deck = deckList.Shuffle().Select(x =>
        {
            var card = TestSetImplementations.GetImplementation(x.Id, this);
            card.MoveToZone(CardZone.Deck);
            return card;
        });
        **/
        Deck = new(ArraySegment<CardImplementation>.Empty);

        Hand = new();
        Board = new();
        Pit = new();
    }

    public void PlayCard()
    {
    }

    public void CombatPlayer()
    {
    }

    public void CombatPlayerCard()
    {
    }

    public void EndTurn()
    {
    }
}