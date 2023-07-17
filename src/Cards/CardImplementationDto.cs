using System;
using System.Linq;
using System.Resources;
using System.Text;
using Serilog;

namespace OpenCCG.Cards;

public record CardImplementationDto
(
    Guid Id,
    CreatureOutline? CreatureOutline,
    CreatureState? CreatureState,
    CreatureAbilities? CreatureAbilities,
    SpellOutline? SpellOutline,
    SpellState? SpellState
)
{
    public bool IsCreature => CreatureOutline is not null;
    public bool IsSpell => SpellOutline is not null;
    public ICardOutline Outline => IsCreature ? CreatureOutline! : SpellOutline!;
    public ICardState State => IsCreature ? CreatureState! : SpellState!;

    public static CardImplementationDto AsCreature(Guid id, CreatureOutline outline, CreatureState state,
        CreatureAbilities abilities)
    {
        if (string.IsNullOrWhiteSpace(state.AddedText))
        {
            return new CardImplementationDto(id, outline, state, abilities, null, null);
        }

        var stripped = outline.Description.TrimStart('\n');
        var description = state.AddedText + stripped;
        var combinedOutline = outline with { Description = description };
        
        return new CardImplementationDto(id, combinedOutline, state, abilities, null, null);
    }

    public static CardImplementationDto AsSpell(Guid id, SpellOutline outline, SpellState state)
    {
        if (string.IsNullOrWhiteSpace(state.AddedText))
        {
            return new CardImplementationDto(id, null, null, null, outline, state);
        }

        var stripped = outline.Description.TrimStart('\n');
        var description = state.AddedText + stripped;
        var combinedOutline = outline with { Description = description };
        
        return new CardImplementationDto(id, null, null, null, combinedOutline, state);
    }
}