using System;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay;

public class Creature : ICard
{
    public Guid Id { get; } = Guid.NewGuid();

    public readonly CreatureOutline CreatureOutline;

    public readonly CreatureState CreatureState;

    public readonly CreatureAbilities Abilities;

    public ICardOutline Outline => CreatureOutline;

    public ICardState State => CreatureState;

    public Creature(CreatureOutline outline, CreatureState state, CreatureAbilities abilities)
    {
        CreatureOutline = outline;
        CreatureState = state;
        Abilities = abilities;
    }

    public virtual void OnPlay()
    {
    }

    public virtual void OnPlay(ICard other)
    {
    }

    public virtual void OnEnter()
    {
        CreatureState.IsExposed = Abilities.Exposed || Abilities.Defender;
        CreatureState.AttacksAvailable = Abilities.Haste ? CreatureState.MaxAttacksPerTurn : 0;
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

    public void OnUpkeep()
    {
        CreatureState.IsExposed = true;
        CreatureState.AttacksAvailable = CreatureState.MaxAttacksPerTurn;
    }

    public void OnEndStep()
    {
        CreatureState.AttacksAvailable = 0;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        CreatureState.Def -= amount;
    }

    public CardImplementationDto AsDto() =>
        CardImplementationDto.AsCreature(Id, CreatureOutline, CreatureState, Abilities);
}