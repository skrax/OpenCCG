using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Dto;

namespace OpenCCG.Net.ServerNodes;

public record RemoveCardDto(string Id);

public partial class Board : Node, IMessageReceiver<MessageType>
{
    [Rpc]
    public void HandleMessage(string messageJson)
    {
        IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
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
            new RemoveCardDto(cardId.ToString()));
    }

    public Dictionary<string, IObserver>? Observers => null;

    public Func<int, string?, string?> GetExecutor(MessageType messageType)
    {
        throw new NotImplementedException();
    }
}