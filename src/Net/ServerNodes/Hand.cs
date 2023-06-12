using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Dto;

namespace OpenCCG.Net.ServerNodes;

public partial class Hand : Node, IMessageReceiver<MessageType>
{
    public void DrawCard(long peerId, CardGameStateDto cardGameStateJson)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.DrawCard, cardGameStateJson);
    }

    public void RemoveCard(long peerId, Guid id)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard, id);
    }

    public void FailPlayCard(long peerId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.FailPlayCard);
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