using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Matchmaking;
using OpenCCG.Net.Messaging;
using Error = OpenCCG.Net.Messaging.Error;

namespace OpenCCG.Net.Gameplay;

public interface IPlayerState
{
    public void PlayCard();

    public void CombatPlayer();

    public void CombatPlayerCard();

    public void EndTurn();
}

public delegate void SessionCommand();

public partial class Session : Node, IMessageController
{
    public Guid Id { get; } = Guid.NewGuid();

    private readonly Queue<SessionCommand> _commandQueue = new();
    private readonly Dictionary<long, IPlayerState> _playerByPeerId = new();

    public Session(QueuedPlayer player1, QueuedPlayer player2)
    {
    }

    public void Configure(IMessageBroker broker)
    {
    }

    public override void _Process(double delta)
    {
        while (_commandQueue.TryDequeue(out var command)) command.Invoke();
    }

    public void OnPlayCard(MessageContext context)
    {
        
    }

    public void OnCombatPlayer(MessageContext context)
    {
    }

    public void OnCombatPlayerCard(MessageContext context)
    {
    }

    public void OnEndTurn(MessageContext context)
    {
    }

    private bool TryEnqueueCommand(long peerId, Func<IPlayerState, SessionCommand> selector)
    {
        if (!_playerByPeerId.TryGetValue(peerId, out var state))
        {
            return false;
        }

        _commandQueue.Enqueue(selector(state));

        return true;
    }
}