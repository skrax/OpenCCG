using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public partial class StatusPanel : Node, IMessageReceiver<MessageType>
{
    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public void HandleMessageAsync(string messageJson)
    {
        throw new NotImplementedException();
    }

    public Executor? GetExecutor(MessageType messageType)
    {
        return null;
    }

    public void SetEnergy(long peerId, int current, int max)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.SetEnergy,
            new SetEnergyDto(current, max));
    }

    public void SetCardCount(long peerId, int value)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.SetCardCount, value);
    }

    public void SetHealth(long peerId, int value)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.SetHealth, value);
    }
}