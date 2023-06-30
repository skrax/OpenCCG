using System;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards;

public abstract class CardImplementation
{
    protected CardImplementation(ICardOutline outline, PlayerGameState playerGameState, ICardState state)
    {
        Outline = outline;
        PlayerGameState = playerGameState;
        State = state;
        State.Cost = outline.Cost;
        State.Zone = CardZone.None;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public ICardOutline Outline { get; }
    
    public ICardState State { get; set; }

    protected PlayerGameState PlayerGameState { get; }

    public virtual Task OnPlayAsync() => Task.CompletedTask;
    public virtual Task OnPlayAsync(CardImplementation cardImplementation) => Task.CompletedTask;

    public virtual Task OnEnterAsync(CreatureImplementation other) => Task.CompletedTask;

    public virtual Task OnExitAsync(CreatureImplementation other) => Task.CompletedTask;

    public virtual Task OnStartCombatTurnAsync(CreatureImplementation other) => Task.CompletedTask;

    public virtual Task OnEndCombatAsync(CreatureImplementation other) => Task.CompletedTask;

    public void MoveToZone(CardZone zone)
    {
        if (zone == State.Zone) return;

        switch (State.Zone)
        {
            case CardZone.None:
                break;
            case CardZone.Deck:
                PlayerGameState.Deck.Remove(this);
                break;
            case CardZone.Hand:
                PlayerGameState.Hand.Remove(this);
                break;
            case CardZone.Board:
                PlayerGameState.Board.Remove(this);
                break;
            case CardZone.Pit:
                PlayerGameState.Pit.Remove(this);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        State.Zone = zone;

        switch (zone)
        {
            case CardZone.None:
                break;
            case CardZone.Deck:
                PlayerGameState.Deck.AddLast(this);
                break;
            case CardZone.Hand:
                PlayerGameState.Hand.AddLast(this);
                break;
            case CardZone.Board:
                PlayerGameState.Board.AddLast(this);
                break;
            case CardZone.Pit:
                PlayerGameState.Pit.AddLast(this);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
        }
    }

    public abstract CardImplementationDto AsDto();
}