using Godot;
using OpenCCG.Net.Matchmaking;
using Serilog;

namespace OpenCCG.Net;

public partial class TestClient : Messaging.MessageBroker
{
    private ENetMultiplayerPeer? _peer;
    private MatchmakingClient _matchmakingClient;

    public override void _Ready()
    {
        _peer = CreateClient("localhost", 57777);
        _matchmakingClient = new MatchmakingClient();
        _matchmakingClient.Configure(this);
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
        _matchmakingClient.EnqueuePlayer(this);
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