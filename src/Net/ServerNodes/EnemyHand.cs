using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public partial class EnemyHand : Node, IMessageReceiver<MessageType>
{
    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public void HandleMessageAsync(string messageJson)
    {
        throw new NotImplementedException();
    }

    public Executor GetExecutor(MessageType messageType)
    {
        throw new NotImplementedException();
    }

    public void RemoveCard(long peerId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard);
    }

    public void DrawCard(long peerId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.DrawCard);
    }
}