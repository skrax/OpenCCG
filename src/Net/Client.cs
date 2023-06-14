using System;
using System.Linq;
using Godot;
using OpenCCG.Core;

namespace OpenCCG.Net;

public partial class Client : Node
{
    [Export] private Main _main;
    private ENetMultiplayerPeer _peer;

    public override void _Ready()
    {
        _peer = new ENetMultiplayerPeer();
        var result = _peer.CreateClient("127.0.0.1", 8080);

        if (result is Error.Ok)
        {
            var mp = GetTree().GetMultiplayer();
            mp.MultiplayerPeer = _peer;

            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ServerDisconnected += OnServerDisconnected;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }
        else if (result is Error.AlreadyInUse)
        {
            _peer.Close();
        }
        else if (result is Error.CantCreate)
        {
            throw new ApplicationException("Failed to connect to server");
        }
    }

    public override void _ExitTree()
    {
        _peer.DisconnectPeer(1);
    }

    private void OnServerDisconnected()
    {
        Logger.Info<Client>("Server disconnected");
    }

    private void OnConnectedToServer()
    {
        Logger.Info<Client>("Connected to server");

        _main.Enqueue();
    }

    private void OnPeerConnected(long id)
    {
        Logger.Info<Client>($"Peer connected {id}");
        GetParent().SetMultiplayerAuthority(Multiplayer.GetUniqueId(), false);
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Info<Client>($"Peer disconnected {id}");
    }
}