using System.Collections.Generic;

namespace OpenCCG.Net.Gameplay;

public class Deck : LinkedList<ICard>
{
    public Deck(IEnumerable<ICard> cards) : base(cards)
    {
    }
}