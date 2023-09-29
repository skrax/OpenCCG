using OpenCCG.Proto;

namespace OpenCCG.Gameserver.Entities;


public class Player
{
    public readonly DeckList DeckList;
    public readonly LinkedList<Card> Deck = new();
    public readonly LinkedList<Card> Hand = new();
    public readonly LinkedList<Card> Board = new();
    public readonly LinkedList<Card> Pit = new();

    public Player(DeckList deckList)
    {
        DeckList = deckList;
    }
}