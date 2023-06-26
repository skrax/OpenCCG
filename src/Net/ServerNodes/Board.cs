using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public record RemoveCardDto(Guid Id);

public record PlayCombatDto(Guid From, Guid? To, bool IsAvatar);

public partial class Board : Node, IMessageReceiver<MessageType>
{
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Dictionary<string, IObserver> Observers { get; } = new();

    public Executor GetExecutor(MessageType messageType)
    {
        throw new NotImplementedException();
    }

    public void PlaceCard(long peerId, CardGameStateDto cardGameStateDtoJson)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.PlaceCard, cardGameStateDtoJson);
    }

    public async Task UpdateCardAsync(long peerId, CardGameStateDto cardGameStateDtoJson)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.UpdateCard, cardGameStateDtoJson);
    }

    public async Task PlayCombatAnimAsync(long peerId, Guid from, Guid to)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.PlayCombatAnim,
            new PlayCombatDto(from, to, false));
    }

    public async Task PlayCombatAvatarAnimAsync(long peerId, Guid from)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.PlayCombatAnim,
            new PlayCombatDto(from, null, true));
    }

    public void RemoveCard(long peerId, Guid cardId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard,
            new RemoveCardDto(cardId));
    }
}