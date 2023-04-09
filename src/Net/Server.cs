using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Api;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public partial class Server : Node, IMainRpc
{
    private readonly GameState _gameState = new();

    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateServer(8080, maxClients: 2);

        if (result is Error.Ok)
        {
            Logger.Info<Server>("listening on port 8080");
            GetTree().GetMultiplayer().MultiplayerPeer = peer;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }
        else if (result is Error.AlreadyInUse)
        {
            peer.Close();
        }
        else if (result is Error.CantCreate)
        {
            throw new ApplicationException("Failed to create server");
        }
    }

    private void OnPeerConnected(long id)
    {
        Logger.Info<Server>($"Peer connected {id}");

        var peers = Multiplayer.GetPeers();
        if (peers.Length == 2)
        {
            var p1 = new PlayerGameState
            {
                PeerId = peers[0],
                EnemyPeerId = peers[1],
                PlayerName = "player-1",
                Nodes = new RpcNodes
                {
                    Hand = GetNode<Hand>("Hand"),
                    EnemyHand = GetNode<EnemyHand>("EnemyHand"),
                    Board = GetNode<Board>("Board"),
                    EnemyBoard = GetNode<EnemyBoard>("EnemyBoard"),
                    StatusPanel = GetNode<ServerNodes.StatusPanel>("Hand/StatusPanel"),
                    EnemyStatusPanel = GetNode<ServerNodes.StatusPanel>("EnemyHand/StatusPanel")
                }
            };
            var p2 = new PlayerGameState
            {
                PeerId = peers[1],
                EnemyPeerId = peers[0],
                PlayerName = "player-2",
                Nodes = new RpcNodes
                {
                    Hand = GetNode<Hand>("Hand"),
                    EnemyHand = GetNode<EnemyHand>("EnemyHand"),
                    Board = GetNode<Board>("Board"),
                    EnemyBoard = GetNode<EnemyBoard>("EnemyBoard"),
                    StatusPanel = GetNode<ServerNodes.StatusPanel>("Hand/StatusPanel"),
                    EnemyStatusPanel = GetNode<ServerNodes.StatusPanel>("EnemyHand/StatusPanel")
                }
            };

            var p1DeckList = new List<CardRecord>();
            var p2DeckList = new List<CardRecord>();
            for (var i = 0; i < 9; ++i)
            {
                var record = CardDatabase.Cards[GD.RandRange(0, CardDatabase.Cards.Length - 1)];
                p1DeckList.Add(record);

                var record2 = CardDatabase.Cards[GD.RandRange(0, CardDatabase.Cards.Length - 1)];
                p2DeckList.Add(record2);
            }

            p1.Init(p1DeckList);
            p2.Init(p2DeckList);

            p1.Enemy = p2;
            p2.Enemy = p1;

            _gameState.PlayerGameStates.Add(p1.PeerId, p1);
            _gameState.PlayerGameStates.Add(p2.PeerId, p2);

            p1.Start();
            p2.Start();

        }
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Info<Server>($"Peer disconnected {id}");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void PlayCard(string id)
    {
        var peerId = Multiplayer.GetRemoteSenderId();

        _gameState.PlayerGameStates[peerId].PlayCard(Guid.Parse(id));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void CombatPlayerCard(string attackerId, string targetId)
    {
        var peerId = Multiplayer.GetRemoteSenderId();

        _gameState.PlayerGameStates[peerId].Combat(Guid.Parse(attackerId), Guid.Parse(targetId));
    }
}