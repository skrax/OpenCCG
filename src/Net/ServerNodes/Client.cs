using System;
using Godot;
using Serilog;

namespace OpenCCG.Net.ServerNodes;

public partial class Client : Node
{
    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateClient("127.0.0.1", 8080);

        if (result is Godot.Error.Ok)
        {
            var mp = GetTree().GetMultiplayer();
            mp.MultiplayerPeer = peer;


            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ServerDisconnected += OnServerDisconnected;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }
        else if (result is Godot.Error.AlreadyInUse)
        {
            peer.Close();
        }
        else if (result is Godot.Error.CantCreate)
        {
            throw new ApplicationException("Failed to connect to server");
        }
    }

    private void OnServerDisconnected()
    {
        Log.Information("Server disconnected");
    }

    private void OnConnectedToServer()
    {
        Log.Information("Connected to server");
    }

    private void OnPeerConnected(long id)
    {
        Log.Information($"Peer connected {id}");

        GetParent().SetMultiplayerAuthority(Multiplayer.GetUniqueId(), false);
    }

    private void OnPeerDisconnected(long id)
    {
        Log.Information($"Peer disconnected {id}");
    }
}