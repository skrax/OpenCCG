namespace OpenCCG.Cards;

public class CreatureState : ICardState
{
    public int Cost { get; set; }

    public int Atk { get; set; }

    public int Def { get; set; }

    public int AttacksAvailable { get; set; }

    public int MaxAttacksPerTurn { get; set; }

    public bool IsExposed { get; set; }

    public CardZone Zone { get; set; }
}