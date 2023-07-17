namespace OpenCCG.Cards;

public class CreatureState : ICardState
{
    public int Atk { get; set; }

    public int Def { get; set; }

    public int AttacksAvailable { get; set; }

    public int MaxAttacksPerTurn { get; set; }

    public bool IsExposed { get; set; }
    public int Cost { get; set; }

    public CardZone Zone { get; set; }

    public string AddedText { get; set; } = string.Empty;

    public CreatureState Copy() => new()
    {
       Atk = Atk,
       Def = Def,
       AttacksAvailable = AttacksAvailable,
       MaxAttacksPerTurn = MaxAttacksPerTurn,
       IsExposed = IsExposed,
       Cost = Cost,
       Zone = Zone,
       AddedText = new(AddedText)
    };
}