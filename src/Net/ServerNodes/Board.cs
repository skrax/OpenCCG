using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public record RemoveCardDto(Guid Id);

public partial class Board : Node, IMessageReceiver<MessageType>
{
    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Dictionary<string, IObserver>? Observers => null;

    public Executor GetExecutor(MessageType messageType)
    {
        throw new NotImplementedException();
    }

    public void PlaceCard(long peerId, CardGameStateDto cardGameStateDtoJson)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.PlaceCard, cardGameStateDtoJson);
    }

    public void UpdateCard(long peerId, CardGameStateDto cardGameStateDtoJson)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.UpdateCard, cardGameStateDtoJson);
    }

    public void RemoveCard(long peerId, Guid cardId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard,
            new RemoveCardDto(cardId));
    }
}