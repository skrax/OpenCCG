using System;
using System.Collections.Generic;
using OpenCCG.Cards;
using OpenCCG.Core.Serilog;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Gameplay;

public class PlayerState
{
    private ILogger _logger;
    private readonly Queue<Action> _commandQueue;
    private readonly Session _session;
    private readonly IMessageBroker _broker;

    public readonly long PeerId;
    public readonly long EnemyPeerId;
    public PlayerState Enemy = null!;
    public readonly List<ICardOutline> DeckList;
    public readonly Deck Deck;
    public readonly Hand Hand;
    public readonly Board Board;
    public readonly Pit Pit;

    public IEnumerable<ICard> AllCards()
    {
        foreach (var card in Deck)
        {
            yield return card;
        }

        foreach (var card in Hand)
        {
            yield return card;
        }

        foreach (var card in Board)
        {
            yield return card;
        }

        foreach (var card in Pit)
        {
            yield return card;
        }
    }

    public bool IsTurn;
    public int Health;
    public int Energy;
    public int MaxEnergy;

    public PlayerState(long peerId, long enemyPeerId, List<ICardOutline> deckList, Session session, IMessageBroker broker)
    {
        PeerId = peerId;
        EnemyPeerId = enemyPeerId;
        DeckList = deckList;
        // TODO
        /**
        var deck = deckList.Shuffle().Select(x =>
        {
            var card = TestSetImplementations.GetImplementation(x.Id, this);
            card.MoveToZone(CardZone.Deck);
            return card;
        });
        **/
        Deck = new(ArraySegment<ICard>.Empty);

        Hand = new();
        Board = new();
        Pit = new();
        IsTurn = false;
        Health = Rules.InitialHealth;
        Energy = Rules.InitialEnergy;
        MaxEnergy = Rules.InitialEnergy;
        _commandQueue = new();
        _session = session;
        _broker = broker;
        _logger = Log.ForContext(new SessionContextEnricher(session.Context));
    }

    public MessageControllerResult PlayCard()
    {
        if (!IsTurn) return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));

        return MessageControllerResult.AsResult();
    }

    public MessageControllerResult CombatPlayer()
    {
        if (!IsTurn) return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));

        return MessageControllerResult.AsResult();
    }

    public MessageControllerResult CombatPlayerCard()
    {
        if (!IsTurn) return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));

        return MessageControllerResult.AsResult();
    }

    public MessageControllerResult EndTurn()
    {
        if (!IsTurn) return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        _logger.Information("Ending turn for peer {PeerId}", PeerId);

        IsTurn = false;

        foreach (var creature in Board)
        {
            creature.OnEndStep();
        }

        foreach (var card in AllCards())
        {
            card.OnEndTurn();
        }

        _commandQueue.Enqueue(Enemy.StartTurn);

        return MessageControllerResult.AsResult();
    }

    public void StartTurn()
    {
        if (IsTurn) return;
        _logger.Information("Starting turn for peer {PeerId}", PeerId);

        IsTurn = true;
        if (MaxEnergy < Rules.MaxEnergy) MaxEnergy += Rules.EnergyGainedPerTurn;
        Energy = MaxEnergy;

        foreach (var creature in Board)
        {
            creature.OnUpkeep();
        }

        foreach (var card in AllCards())
        {
            card.OnStartTurn();
        }
        
        _broker.EnqueueMessage(PeerId, Message.Create(Route.EnableEndTurnButton));
    }

    public void Process()
    {
        while (_commandQueue.TryDequeue(out var command)) command.Invoke();
    }
}