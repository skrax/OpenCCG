using System;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards;

public abstract class CardImplementation
{
    protected CardImplementation(ICardOutline outline, PlayerGameState2 playerGameState)
    {
        Outline = outline;
        PlayerGameState = playerGameState;
        Cost = outline.Cost;
        Zone = CardZone.None;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public ICardOutline Outline { get; }

    public int Cost { get; set; }

    public CardZone Zone { get; set; }

    protected PlayerGameState2 PlayerGameState { get; }

    public virtual Task OnPlayAsync() => Task.CompletedTask;
    public virtual Task OnPlayAsync(CardImplementation cardImplementation) => Task.CompletedTask;

    public virtual Task OnEnterAsync(CreatureImplementation other) => Task.CompletedTask;

    public virtual Task OnExitAsync(CreatureImplementation other) => Task.CompletedTask;

    public virtual Task OnStartCombatTurnAsync(CreatureImplementation other) => Task.CompletedTask;

    public virtual Task OnEndCombatAsync(CreatureImplementation other) => Task.CompletedTask;

    public void MoveToZone(CardZone zone)
    {
        if (zone == Zone) return;

        switch (Zone)
        {
            case CardZone.None:
                break;
            case CardZone.Deck:
                PlayerGameState.Deck.Remove(this);
                break;
            case CardZone.Hand:
                return;
            case CardZone.Board:
                PlayerGameState.Board.Remove(this);
                break;
            case CardZone.Pit:
                PlayerGameState.Pit.Remove(this);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Zone = zone;

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

    public CardImplementationDto AsDto()
    {
        return new CardImplementationDto();
    }
}

public record CardImplementationDto();