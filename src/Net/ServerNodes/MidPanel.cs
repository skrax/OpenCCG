using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public partial class MidPanel : Node, IMessageReceiver<MessageType>
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

    public void EndTurnButtonSetActive(long peerId, EndTurnButtonSetActiveDto dto)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.EndTurnButtonSetActive, dto);
    }
}