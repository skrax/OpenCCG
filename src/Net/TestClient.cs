using System;
using Godot;
using OpenCCG.Net.Matchmaking;
using Serilog;

namespace OpenCCG.Net;

public partial class TestClient : Messaging.MessageBroker
{
    private ENetMultiplayerPeer? _peer;
    private readonly MatchmakingClient _matchmakingClient;

    public TestClient()
    {
        _matchmakingClient = new(this);
    }

    public override void _Ready()
    {
        _peer = CreateClient("localhost", 57777);
        _matchmakingClient.Configure();
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
        var deck = new SavedDeck("", Array.Empty<CardUIDeck.JsonRecord>());
        _matchmakingClient.EnqueuePlayer(deck);
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