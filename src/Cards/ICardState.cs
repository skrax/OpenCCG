namespace OpenCCG.Cards;

public interface ICardState
{
    int Cost { get; set; }
    CardZone Zone { get; set; }
}