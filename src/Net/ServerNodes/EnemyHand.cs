using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Net.Rpc;

namespace OpenCCG.Net.ServerNodes;

public partial class EnemyHand : Node, IMessageReceiver<MessageType>
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

    public void RemoveCard(long peerId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, peerId, MessageType.RemoveCard);
    }

    public async Task DrawCard(long peerId)
    {
        await IMessageReceiver<MessageType>.GetAsync(this, peerId, MessageType.DrawCard);
    }
}