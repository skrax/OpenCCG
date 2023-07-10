using System;

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
        return new CardImplementationDto(id, outline, state, abilities, null, null);
    }

    public static CardImplementationDto AsSpell(Guid id, SpellOutline outline, SpellState state)
    {
        return new CardImplementationDto(id, null, null, null, outline, state);
    }
}