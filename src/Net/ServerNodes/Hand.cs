using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public partial class Hand : Node, IMessageReceiver<MessageType>
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

    public void DrawCard(long peerId, CardImplementationDto dto)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.DrawCard, dto);
    }

    public void RemoveCard(long peerId, Guid id)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard, id);
    }
}