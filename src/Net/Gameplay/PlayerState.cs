using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Core.Serilog;
using OpenCCG.Net.Gameplay.Dto;
using OpenCCG.Net.Gameplay.Test;
using OpenCCG.Net.Messaging;
using Serilog;
using Error = OpenCCG.Net.Messaging.Error;

namespace OpenCCG.Net.Gameplay;

public class PlayerState
{
    private readonly ILogger _logger;
    private readonly Queue<Action> _commandQueue;
    private readonly Session _session;
    private readonly IMessageBroker _broker;

    public Guid Id = Guid.NewGuid();
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

    public PlayerState(long peerId, long enemyPeerId, List<ICardOutline> deckList, Session session,
        IMessageBroker broker)
    {
        PeerId = peerId;
        EnemyPeerId = enemyPeerId;
        DeckList = deckList;
        Deck = new();
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

    public void SetupMatch()
    {
        _logger.Information("Setting up match for peer {PeerId}", PeerId);
        Deck.Clear();
        Hand.Clear();
        Board.Clear();
        Pit.Clear();
        IsTurn = false;
        // TODO add cards to deck

        foreach (var card in DeckList.Select(x => TestSetImplementations.GetImplementation(x.Id)).Shuffle())
        {
            Deck.AddLast(card);
        }

        _commandQueue.Clear();

        SetMaxEnergy(Rules.InitialEnergy);
        SetEnergy(Rules.InitialEnergy);
        SetHealth(Rules.InitialHealth);
        SyncCardCount();

        Draw(Rules.InitialCardsDrawn);
    }

    public void SetHealth(int amount)
    {
        Health = amount;
        EnqueueSyncMessage(Route.SetHealth, new PlayerMetricDto(Id, Health));
    }

    public void SetEnergy(int amount)
    {
        Energy = amount;
        EnqueueSyncMessage(Route.SetEnergy, new PlayerMetricDto(Id, Energy, MaxEnergy));
    }

    public void SetMaxEnergy(int amount)
    {
        MaxEnergy = amount;
        EnqueueSyncMessage(Route.SetEnergy, new PlayerMetricDto(Id, Energy, MaxEnergy));
    }

    public void Draw(int count)
    {
        foreach (var card in Deck.Take(count).ToList())
        {
            Deck.Remove(card);
            Hand.AddLast(card);

            var dto = card.AsDto();
            EnqueueSyncMessage(Route.AddCardToHand, new AddCardDto(Id, dto), new AddCardDto(Id, null));
            SyncCardCount();
        }
    }

    private void SyncCardCount()
    {
        EnqueueSyncMessage(Route.SetCardsInDeck, new PlayerMetricDto(Id, Deck.Count));
        EnqueueSyncMessage(Route.SetCardsInHand, new PlayerMetricDto(Id, Hand.Count));
    }

    public MessageControllerResult PlayCard(Guid cardId)
    {
        if (!IsTurn)
        {
            var error = new Error(ErrorCode.Conflict, "Cards can only played during a players turn.");
            return MessageControllerResult.AsError(error);
        }

        if (Hand.SingleOrDefault(x => x.Id == cardId) is not Creature card)
        {
            var error = new Error(ErrorCode.BadRequest, "CardId not found");
            return MessageControllerResult.AsError(error);
        }

        card.State.Zone = CardZone.Board;
        Hand.Remove(card);
        Board.AddLast(card);
        var removeCardMessage = Message.Create(Route.RemoveCardFromHand, new RemoveCardDto(Id, card.Id));
        _commandQueue.Enqueue(() => _broker.EnqueueMessage(PeerId, removeCardMessage));
        //_commandQueue.Enqueue(() => _broker.EnqueueMessage(PeerId, Message.Create(Route.AddCardToBoard, card.AsDto())));

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
        if (MaxEnergy < Rules.MaxEnergy)
        {
            SetMaxEnergy(MaxEnergy + Rules.EnergyGainedPerTurn);
        }

        SetEnergy(MaxEnergy);

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

    private void EnqueueSyncMessage<T>(string route, T myself, T? other = default)
    {
        var message = Message.Create(route, myself);
        var messageOther = Message.Create(route, other ?? myself);
        _broker.EnqueueMessage(PeerId, message);
        _broker.EnqueueMessage(EnemyPeerId, messageOther);
    }
}