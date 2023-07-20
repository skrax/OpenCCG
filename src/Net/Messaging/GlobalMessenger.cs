using System;
using System.Text.Json;
using Godot;
using Serilog;

namespace OpenCCG.Net.Messaging;

public partial class GlobalMessenger : Node
{
    public Action<Message>? OnReceived;

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void HandleMessage(string messageJson)
    {
        var message = JsonSerializer.Deserialize<Message>(messageJson);
        if (message == null)
        {
            Log.Warning("Received invalid message: {Message}", messageJson);
            return;
        }

        OnReceived?.Invoke(message);
    }

    public Godot.Error SendMessage(long peerId, Message message)
    {
        var messageJson = JsonSerializer.Serialize(message);
        return RpcId(peerId, nameof(HandleMessage), messageJson);
    }
}