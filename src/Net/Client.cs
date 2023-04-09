using System;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Api;

namespace OpenCCG.Net;

public partial class Client : Node, IClientRpc
{
    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateClient("127.0.0.1", 8080);

        if (result is Error.Ok)
        {
            var mp = GetTree().GetMultiplayer();
            mp.MultiplayerPeer = peer;


            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ServerDisconnected += OnServerDisconnected;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }
        else if (result is Error.AlreadyInUse)
        {
            peer.Close();
        }
        else if (result is Error.CantCreate)
        {
            throw new ApplicationException("Failed to connect to server");
        }
    }

    private void OnServerDisconnected()
    {
        Logger.Info<Client>("Server disconnected");
    }

    private void OnConnectedToServer()
    {
        Logger.Info<Client>("Connected to server");
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