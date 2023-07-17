namespace OpenCCG.Cards;

public class CreatureAbilities
{
    public bool Exposed { get; set; }

    public bool Haste { get; set; }

    public bool Drain { get; set; }

    public bool Defender { get; set; }

    public bool Arcane { get; set; }

    public CreatureAbilities Copy() => new()
    {
        Exposed = Exposed,
        Haste = Haste,
        Drain = Drain,
        Defender = Defender,
        Arcane = Arcane
    };
}