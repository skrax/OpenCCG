using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
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

    public Executor? GetExecutor(MessageType messageType)
    {
        return null;
    }

    public async Task PlaceCard(long peerId, CardImplementationDto dto)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.PlaceCard, dto);
    }

    public async Task UpdateCardAsync(long peerId, CardImplementationDto dto)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.UpdateCard, dto);
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

    public async Task RemoveCard(long peerId, Guid cardId)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.RemoveCard,
            new RemoveCardDto(cardId));
    }
}