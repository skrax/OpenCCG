using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Core;
using OpenCCG.Core.Serilog;
using OpenCCG.Net.Matchmaking;
using OpenCCG.Net.Messaging;
using Serilog;
using Error = OpenCCG.Net.Messaging.Error;

namespace OpenCCG.Net.Gameplay;

public partial class Session : Node, IMessageController
{
    public readonly SessionContext Context;
    private readonly ILogger _logger;
    private readonly Queue<SessionCommand> _commandQueue = new();
    private readonly Dictionary<long, PlayerState> _playerByPeerId = new();

    public Session(QueuedPlayer queuedPlayer1, QueuedPlayer queuedPlayer2)
    {
        Context = new SessionContext(Guid.NewGuid());
        _logger = Log.ForContext(new SessionContextEnricher(Context));
        
        var player1 = new PlayerState(queuedPlayer1.PeerId, queuedPlayer2.PeerId, queuedPlayer1.DeckList);
        var player2 = new PlayerState(queuedPlayer2.PeerId, queuedPlayer1.PeerId, queuedPlayer2.DeckList);
        player1.Enemy = player2;
        player2.Enemy = player1;
        _playerByPeerId.Add(player1.PeerId, player1);
        _playerByPeerId.Add(player2.PeerId, player2);
        
        Name = $"[Session]{queuedPlayer1.PeerId}_{queuedPlayer2.PeerId}";
    }

    public void Configure(IMessageBroker broker)
    {
        broker.Map(Route.PlayCard, OnPlayCard);
        broker.Map(Route.CombatPlayer, OnCombatPlayer);
        broker.Map(Route.CombatPlayerCard, OnCombatPlayerCard);
        broker.Map(Route.EndTurn, OnEndTurn);
        _logger.Information("{SessionName} created", Name);

        foreach (var peerIds in _playerByPeerId.Keys)
        {
            broker.EnqueueMessage(peerIds, Message.Create(Route.MatchFound, Context));
        }
    }

    public override void _Process(double delta)
    {
        while (_commandQueue.TryDequeue(out var command)) command.Invoke();
    }

    private MessageControllerResult OnPlayCard(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(player.PlayCard);

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnCombatPlayer(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(player.CombatPlayer);

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnCombatPlayerCard(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(player.CombatPlayerCard);

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnEndTurn(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(player.EndTurn);

        return MessageControllerResult.AsResult();
    }
}