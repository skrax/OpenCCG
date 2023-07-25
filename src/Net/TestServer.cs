using Godot;
using OpenCCG.Net.Gameplay;
using OpenCCG.Net.Matchmaking;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net;

public partial class TestServer : MessageBroker
{
    private ENetMultiplayerPeer? _peer;

    [Export] private MatchmakingService _matchmakingService = null!;
    [Export] private SessionManager _sessionManager = null!;

    public override void _Ready()
    {
        _peer = CreateServer(57777, 32);
    }

    protected override void OnPeerConnected(long id)
    {
        Log.Information("Peer {Id} connected", id);
    }

    protected override void OnPeerDisconnected(long id)
    {
        Log.Information("Peer {Id} disconnected", id);
        _sessionManager.DissolveSession(id);
    }

    protected override void OnConnectionFailed()
    {
        Log.Error("Connection failed");
    }

    protected override void OnConnectedToServer()
    {
        Log.Information("Connected to server");
    }

    protected override void OnServerDisconnected()
    {
        Log.Warning("Server disconnected");
    }
}