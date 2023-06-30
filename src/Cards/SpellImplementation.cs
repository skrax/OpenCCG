using OpenCCG.Net;

namespace OpenCCG.Cards;

public abstract class SpellImplementation : CardImplementation
{
    protected SpellImplementation(ICardOutline outline, PlayerGameState2 playerGameState, ISpellOutline outline1) :
        base(outline, playerGameState)
    {
        Outline = outline1;
    }

    public new ISpellOutline Outline { get; }
}