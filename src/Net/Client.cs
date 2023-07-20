using System;
using Godot;
using Serilog;

namespace OpenCCG.Net;

public partial class Client : Node
{
    [Export] private Main _main;
    private ENetMultiplayerPeer _peer;
    [Export] private string _serverAddress;

    public override void _Ready()
    {
        _peer = new ENetMultiplayerPeer();
        var result = _peer.CreateClient(_serverAddress, 57618);

        if (result is Godot.Error.Ok)
        {
            var mp = GetTree().GetMultiplayer();
            mp.MultiplayerPeer = _peer;

            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ServerDisconnected += OnServerDisconnected;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
            Multiplayer.ConnectionFailed += OnConnectionFailed;
        }
        else if (result is Godot.Error.AlreadyInUse)
        {
            _peer.Close();
        }
        else if (result is Godot.Error.CantCreate)
        {
            throw new ApplicationException("Failed to connect to server");
        }
    }

    private void OnConnectionFailed()
    {
        Log.Error("Connection failed");
    }

    public override void _ExitTree()
    {
        _peer.DisconnectPeer(1);
    }

    private void OnServerDisconnected()
    {
        Log.Information("Server disconnected");
    }

    private void OnConnectedToServer()
    {
        Log.Information("Connected to server");

        _main.Enqueue();
    }

    private void OnPeerConnected(long id)
    {
        Log.Information("Peer connected {Id}", id);
    }

    private void OnPeerDisconnected(long id)
    {
        Log.Information("Peer disconnected {Id}", id);
    }
}