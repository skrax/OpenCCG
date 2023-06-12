using System;
using System.Collections.Generic;
using Godot;

namespace OpenCCG.Net.ServerNodes;

public partial class MidPanel : Node, IMessageReceiver<MessageType>
{
    public void EndTurnButtonSetActive(long peerId, bool isActive)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.EndTurnButtonSetActive, isActive);
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