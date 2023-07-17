namespace OpenCCG.Cards;

public class SpellState : ICardState
{
    public int Cost { get; set; }

    public CardZone Zone { get; set; }
    
    public string AddedText { get; set; }
}