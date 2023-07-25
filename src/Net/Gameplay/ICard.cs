using System;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay;

public interface ICard
{
    public Guid Id { get; }

    public ICardOutline Outline { get; }

    public ICardState State { get; }

    public void OnPlay();

    public void OnPlay(ICard other);

    public void OnEnter(ICard other);

    public void OnExit(ICard other);

    public void OnStartCombat(ICard other);

    public void OnEndCombat(ICard other);

    public void OnStartTurn();

    public void OnEndTurn();

    public void AsDto();
}