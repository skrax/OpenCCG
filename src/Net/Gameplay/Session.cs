using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Matchmaking;
using OpenCCG.Net.Messaging;

namespace OpenCCG.Net.Gameplay;

public partial class Session : Node, IMessageController
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public IPlayerState Player1 { get; }
    public IPlayerState Player2 { get; }

    private readonly Queue<SessionCommand> _commandQueue = new();
    private readonly Dictionary<long, IPlayerState> _playerByPeerId = new();

    public Session(QueuedPlayer player1, QueuedPlayer player2)
    {
        // TODO
        _playerByPeerId.Add(player1.PeerId, null!);
        _playerByPeerId.Add(player2.PeerId, null!);
        Name = $"[Session]{player1.PeerId}_{player2.PeerId}";
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