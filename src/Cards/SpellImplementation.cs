using OpenCCG.Net;

namespace OpenCCG.Cards;

public abstract class SpellImplementation : CardImplementation
{
    protected SpellImplementation(SpellOutline outline, PlayerGameState playerGameState) :
        base(outline, playerGameState,
            new SpellState
            {
                Cost = outline.Cost,
                Zone = CardZone.Deck
            })
    {
    }
    

    public override CardImplementationDto AsDto() => CardImplementationDto.AsSpell(Id, SpellOutline, SpellState);

    public SpellOutline SpellOutline => (SpellOutline)Outline;

    public SpellState SpellState => (SpellState)State;
}