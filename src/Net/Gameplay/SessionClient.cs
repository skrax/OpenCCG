using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Core;
using OpenCCG.Core.Serilog;
using OpenCCG.GameBoard;
using OpenCCG.Net.Gameplay.Dto;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Gameplay;

public partial class SessionClient : Node
{
    private readonly MessageBroker _broker;
    private readonly GameBoard.MidPanel _midPanel;
    private readonly GameBoard.StatusPanel _statusPanel;
    private readonly GameBoard.StatusPanel _enemyStatusPanel;
    private readonly GameBoard.Hand _hand;
    private readonly GameBoard.EnemyHand _enemyHand;
    private readonly Queue<Action> _commandQueue = new();
    private readonly MatchInfoDto _matchInfoDto;
    private readonly ILogger _logger;

    public SessionClient(MessageBroker broker,
        MatchInfoDto matchInfoDto,
        GameBoard.MidPanel midPanel,
        GameBoard.StatusPanel statusPanel,
        GameBoard.StatusPanel enemyStatusPanel,
        GameBoard.Hand hand,
        EnemyHand enemyHand
    )
    {
        _broker = broker;
        _midPanel = midPanel;
        _matchInfoDto = matchInfoDto;
        _statusPanel = statusPanel;
        _enemyStatusPanel = enemyStatusPanel;
        _hand = hand;
        _enemyHand = enemyHand;
        _logger = Log.ForContext(new SessionContextEnricher(new(matchInfoDto.SessionId)));
        Name = $"[{_matchInfoDto.SessionId}]{_matchInfoDto.PlayerId}_{_matchInfoDto.EnemyPlayerId}";
    }

    public override void _Ready()
    {
        Configure();
    }

    public override void _Process(double delta)
    {
        while (_commandQueue.TryDequeue(out var command))
        {
            command();
        }
    }

    public override void _ExitTree()
    {
        // TODO
    }

    public void Configure()
    {
        _broker.MapAwaitableResponse(Route.PlayCardResponse);
        _broker.MapAwaitableResponse(Route.CombatPlayerResponse);
        _broker.MapAwaitableResponse(Route.CombatPlayerCardResponse);
        _broker.MapAwaitableResponse(Route.EndTurnResponse);
        _broker.Map(Route.EnableEndTurnButton, OnEnableEndTurnButton);
        _broker.Map(Route.AddCardToHand, OnAddCardToHand);
        _broker.Map(Route.AddCardToBoard, OnAddCardToBoard);
        _broker.Map(Route.RemoveCardFromHand, OnRemoveCardFromHand);
        _broker.Map(Route.RemoveCardFromBoard, OnRemoveCardFromBoard);
        _broker.Map(Route.SetEnergy, OnSetEnergy);
        _broker.Map(Route.SetHealth, OnSetHealth);
        _broker.Map(Route.SetCardsInHand, OnSetCardsInHand);
        _broker.Map(Route.SetCardsInDeck, OnSetCardsInDeck);
        _logger.Information(Logging.Templates.ServiceIsRunning, nameof(SessionClient));
    }

    private MessageControllerResult OnSetCardsInDeck(MessageContext context)
    {
        if (context.Message.TryUnwrap(out PlayerMetricDto metric))
        {
            // TODO
        }

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnSetCardsInHand(MessageContext context)
    {
        if (context.Message.TryUnwrap(out PlayerMetricDto metric))
        {
            if (metric.PlayerId == _matchInfoDto.PlayerId)
            {
                _statusPanel.SetCardCount(metric.Value);
            }
            else if (metric.PlayerId == _matchInfoDto.EnemyPlayerId)
            {
                _enemyStatusPanel.SetCardCount(metric.Value);
            }
        }

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnSetHealth(MessageContext context)
    {
        if (context.Message.TryUnwrap(out PlayerMetricDto metric))
        {
            if (metric.PlayerId == _matchInfoDto.PlayerId)
            {
                _statusPanel.SetHealth(metric.Value);
            }
            else if (metric.PlayerId == _matchInfoDto.EnemyPlayerId)
            {
                _enemyStatusPanel.SetHealth(metric.Value);
            }
        }

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnSetEnergy(MessageContext context)
    {
        if (context.Message.TryUnwrap(out PlayerMetricDto metric))
        {
            if (metric.PlayerId == _matchInfoDto.PlayerId)
            {
                _statusPanel.SetEnergy(metric.Value, metric.MaxValue!.Value);
            }
            else if (metric.PlayerId == _matchInfoDto.EnemyPlayerId)
            {
                _enemyStatusPanel.SetEnergy(metric.Value, metric.MaxValue!.Value);
            }
        }

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnRemoveCardFromBoard(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnRemoveCardFromHand(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnAddCardToBoard(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnAddCardToHand(MessageContext context)
    {
        if (context.Message.TryUnwrap(out AddCardDto addCardDto))
        {
            _logger.Information("Add card to hand: {PlayerId} {CardId}", addCardDto.PlayerId, addCardDto.Dto?.Id);
            if (addCardDto.PlayerId == _matchInfoDto.PlayerId)
            {
                _hand.AddCard(addCardDto.Dto!);
            }
            else if (addCardDto.PlayerId == _matchInfoDto.EnemyPlayerId)
            {
                _enemyHand.AddCard();
            }
        }
        else
        {
            _logger.Error("Failed to unwrap message for {Route}", Route.AddCardToHand);
        }

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnEnableEndTurnButton(MessageContext context)
    {
        _logger.Information("End turn button enabled");

        _midPanel.EndTurnButtonSetActive(new(true, null));

        return MessageControllerResult.AsResult();
    }
}