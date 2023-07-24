using System.Collections.Generic;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay;

public class Deck : LinkedList<CardImplementation>
{
    public Deck(IEnumerable<CardImplementation> cards) : base(cards)
    {
    }
}