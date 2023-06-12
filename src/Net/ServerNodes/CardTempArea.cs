using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public enum RequireTargetType
{
    All,
    Creature,
    Avatar
}

public enum RequireTargetSide
{
    All,
    Friendly,
    Enemy
}

public record RequireTargetInputDto(CardGameStateDto Card, RequireTargetType Type, RequireTargetSide Side);

public record RequireTargetOutputDto(Guid? cardId);

public partial class CardTempArea : Node, IMessageReceiver<MessageType>
{
    public Dictionary<string, IObserver>? Observers { get; } = new();

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public async void HandleMessageAsync(string messageJson) =>
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);

    public Executor GetExecutor(MessageType messageType)
    {
        throw new NotImplementedException();
    }

    public async Task<RequireTargetOutputDto> RequireTargetsAsync(long peerId, RequireTargetInputDto input)
    {
        return await IMessageReceiver<MessageType>.GetAsync<RequireTargetInputDto, RequireTargetOutputDto>(this,
            peerId,
            MessageType.RequireTarget,
            input
        );
    }

    public void TmpShowCard(long peerId, CardGameStateDto cardGameStateDto)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.TmpShowCard, cardGameStateDto);
    }
}