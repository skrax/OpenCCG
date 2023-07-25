using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Core.Serilog;
using OpenCCG.Net.Gameplay.Dto;
using OpenCCG.Net.Matchmaking;
using OpenCCG.Net.Messaging;
using Serilog;
using Error = OpenCCG.Net.Messaging.Error;

namespace OpenCCG.Net.Gameplay;

public partial class Session : Node
{
    public readonly SessionContext Context;
    private readonly ILogger _logger;
    private readonly Queue<SessionCommandContext> _commandQueue = new();
    private readonly Dictionary<long, PlayerState> _playerByPeerId = new();
    private readonly IMessageBroker _broker;

    public Session(QueuedPlayer queuedPlayer1, QueuedPlayer queuedPlayer2, IMessageBroker broker)
    {
        _broker = broker;
        Context = new SessionContext(Guid.NewGuid());
        _logger = Log.ForContext(new SessionContextEnricher(Context));
        Name = $"[Session]{queuedPlayer1.PeerId}_{queuedPlayer2.PeerId}";

        var player1 = new PlayerState(queuedPlayer1.PeerId, queuedPlayer2.PeerId, queuedPlayer1.DeckList, this, broker);
        var player2 = new PlayerState(queuedPlayer2.PeerId, queuedPlayer1.PeerId, queuedPlayer2.DeckList, this, broker);
        player1.Enemy = player2;
        player2.Enemy = player1;
        _playerByPeerId.Add(player1.PeerId, player1);
        _playerByPeerId.Add(player2.PeerId, player2);
    }

    public void Configure()
    {
        _broker.Map(Route.PlayCard, OnPlayCard);
        _broker.Map(Route.CombatPlayer, OnCombatPlayer);
        _broker.Map(Route.CombatPlayerCard, OnCombatPlayerCard);
        _broker.Map(Route.EndTurn, OnEndTurn);
        _logger.Information("{SessionName} created", Name);
    }

    public void Begin()
    {
        foreach (var playerState in _playerByPeerId.Values)
        {
            var matchInfo = new MatchInfoDto(Context.SessionId, playerState.Id, playerState.Enemy.Id);
            _broker.EnqueueMessage(playerState.PeerId, Message.Create(Route.MatchFound, matchInfo));
        }

        foreach (var playerState in _playerByPeerId.Values)
        {
            playerState.SetupMatch();
        }

        var playerStart = GD.RandRange(0, _playerByPeerId.Keys.Count - 1);

        var peerId = _playerByPeerId.Keys.ElementAt(playerStart);
        _playerByPeerId[peerId].StartTurn();
    }

    public override void _Process(double delta)
    {
        while (_commandQueue.TryDequeue(out var commandContext))
        {
            var result = commandContext.Command.Invoke();
            _broker.SendResult(commandContext.MessageContext, result);

            commandContext.PlayerState.Process();
        }
    }

    private MessageControllerResult? OnPlayCard(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(new(context, player.PlayCard, player));

        return MessageControllerResult.AsDeferred();
    }

    private MessageControllerResult? OnCombatPlayer(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(new(context, player.CombatPlayer, player));

        return MessageControllerResult.AsDeferred();
    }

    private MessageControllerResult? OnCombatPlayerCard(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(new(context, player.CombatPlayerCard, player));

        return MessageControllerResult.AsDeferred();
    }

    private MessageControllerResult? OnEndTurn(MessageContext context)
    {
        if (!_playerByPeerId.TryGetValue(context.PeerId, out var player))
        {
            _logger.Error("Player with {PeerId} not found", context.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.Conflict));
        }

        _commandQueue.Enqueue(new(context, player.EndTurn, player));

        return MessageControllerResult.AsDeferred();
    }
}