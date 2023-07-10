using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public partial class Hand : Node, IMessageReceiver<MessageType>
{
    public Dictionary<string, IObserver>? Observers { get; } = new();

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }


    public Executor? GetExecutor(MessageType messageType)
    {
        return null;
    }

    public async Task DrawCard(long peerId, CardImplementationDto dto)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.DrawCard, dto);
    }

    public void RemoveCard(long peerId, Guid id)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard, id);
    }
}