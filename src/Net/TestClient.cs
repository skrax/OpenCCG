using System;
using System.IO;
using System.Text.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Matchmaking;
using OpenCCG.Net.Messaging;
using Serilog;
using FileAccess = Godot.FileAccess;

namespace OpenCCG.Net;

public partial class TestClient : MessageBroker
{
    private ENetMultiplayerPeer? _peer;
    [Export] private MatchmakingClient _matchmakingClient = null!;
    [Export] private string _serverAddress = "localhost";
    [Export] private int _serverPort = 57777;

    public override void _Ready()
    {
        _peer = CreateClient(_serverAddress, _serverPort);
    }

    protected override void OnPeerConnected(long id)
    {
        Log.Information("Peer {Id} connected", id);
    }

    protected override void OnPeerDisconnected(long id)
    {
        Log.Information("Peer {Id} disconnected", id);
    }

    protected override void OnConnectionFailed()
    {
        Log.Error("Connection failed");
    }

    protected override void OnConnectedToServer()
    {
        Log.Information("Connected to server");
        var runtimeData = this.GetAutoloaded<RuntimeData>();
        runtimeData._deckPath = "user://aggro.deck";

        var password = runtimeData._useQueuePassword ? runtimeData._queuePassword : null;
        using var file = FileAccess.Open(runtimeData._deckPath, FileAccess.ModeFlags.Read);
        if (file == null) throw new FileLoadException();
        var deck = JsonSerializer.Deserialize<SavedDeck>(file.GetAsText())!;

        _matchmakingClient.EnqueuePlayer(deck, password);
    }

    protected override void OnServerDisconnected()
    {
        Log.Warning("Server disconnected");
    }

    public override void _ExitTree()
    {
        _peer?.DisconnectPeer(1);
        base._ExitTree();
    }
}