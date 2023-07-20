using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Matchmaking;
using Serilog;

namespace OpenCCG.Net;




public partial class TestServer : Messaging.MessageBroker
{
    private ENetMultiplayerPeer? _peer;

    public override void _Ready()
    {
        _peer = CreateServer(57777, 32);
        var matchmaking = new MatchmakingService();
        matchmaking.Configure(this);
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
    }

    protected override void OnServerDisconnected()
    {
        Log.Warning("Server disconnected");
    }
}