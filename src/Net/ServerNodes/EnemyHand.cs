using System;
using System.Collections.Generic;
using Godot;

namespace OpenCCG.Net.ServerNodes;

public partial class EnemyHand : Node, IMessageReceiver<MessageType>
{
    public void RemoveCard(long peerId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard);
    }

    public void DrawCard(long peerId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.DrawCard);
    }

    public Dictionary<string, IObserver>? Observers => null;
    
    [Rpc]
    public void HandleMessage(string messageJson)
    {
        throw new NotImplementedException();
    }

    public Executor GetExecutor(MessageType messageType)
    {
        throw new NotImplementedException();
    }
}