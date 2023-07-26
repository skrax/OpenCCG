using System;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay;

public abstract class Spell : ICard
{
    public Guid Id { get; } = Guid.NewGuid();

    public readonly SpellOutline SpellOutline;

    public readonly SpellState SpellState;

    public ICardOutline Outline => SpellOutline;

    public ICardState State => SpellState;

    public Spell(SpellOutline outline)
    {
        SpellOutline = outline;
        SpellState = new SpellState
        {
            Cost = outline.Cost,
            Zone = CardZone.None
        };
    }

    public abstract void OnPlay();

    public virtual void OnPlay(ICard other)
    {
    }

    public virtual void OnEnter(ICard other)
    {
    }

    public virtual void OnExit(ICard other)
    {
    }

    public virtual void OnStartCombat(ICard other)
    {
    }

    public virtual void OnEndCombat(ICard other)
    {
    }

    public virtual void OnStartTurn()
    {
    }

    public virtual void OnEndTurn()
    {
    }

    public CardImplementationDto AsDto() => CardImplementationDto.AsSpell(Id, SpellOutline, SpellState);
}