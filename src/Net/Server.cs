using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public partial class Server : Node, IMessageReceiver<MessageType>
{
    private readonly GameState _gameState = new();

    public Dictionary<string, IObserver>? Observers { get; } = new();

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.PlayCard => Executor.Make<Guid>(PlayCard),
            MessageType.CombatPlayerCard => Executor.Make<CombatPlayerCardDto>(
                CombatPlayerCard),
            MessageType.CombatPlayer => Executor.Make<Guid>(CombatPlayer),
            MessageType.EndTurn => Executor.Make(EndTurn),
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
        };
    }

    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateServer(8080, 2);

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
            var rpcNodes = new RpcNodes
            {
                Server = this,
                Hand = GetNode<Hand>("Hand"),
                EnemyHand = GetNode<EnemyHand>("EnemyHand"),
                Board = GetNode<Board>("Board"),
                EnemyBoard = GetNode<Board>("EnemyBoard"),
                StatusPanel = GetNode<ServerNodes.StatusPanel>("Hand/StatusPanel"),
                EnemyStatusPanel = GetNode<ServerNodes.StatusPanel>("EnemyHand/StatusPanel"),
                MidPanel = GetNode<ServerNodes.MidPanel>("MidPanel"),
                CardTempArea = GetNode<ServerNodes.CardTempArea>("CardTempArea"),
                EnemyCardTempArea = GetNode<ServerNodes.CardTempArea>("EnemyCardTempArea")
            };

            var p1 = new PlayerGameState
            {
                PeerId = peers[0],
                EnemyPeerId = peers[1],
                PlayerName = "player-1",
                Nodes = rpcNodes
            };
            var p2 = new PlayerGameState
            {
                PeerId = peers[1],
                EnemyPeerId = peers[0],
                PlayerName = "player-2",
                Nodes = rpcNodes
            };

            var p1DeckList = new List<CardRecord>();
            var p2DeckList = new List<CardRecord>();
            for (var i = 0; i < 9; ++i)
            {
                var record = Database.Cards[GD.RandRange(0, Database.Cards.Length - 1)];
                p1DeckList.Add(record);

                var record2 = Database.Cards[GD.RandRange(0, Database.Cards.Length - 1)];
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
            p1.StartTurn();
        }
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Info<Server>($"Peer disconnected {id}");
    }

    private async void PlayCard(int senderPeerId, Guid cardId)
    {
        await _gameState.PlayerGameStates[senderPeerId].PlayCard(cardId);
    }

    private void CombatPlayerCard(int senderPeerId, CombatPlayerCardDto t)
    {
        _gameState.PlayerGameStates[senderPeerId].Combat(t.AttackerId, t.TargetId);
    }

    private void CombatPlayer(int senderPeerId, Guid cardId)
    {
        _gameState.PlayerGameStates[senderPeerId].CombatPlayer(cardId);
    }

    private void EndTurn(int senderPeerId)
    {
        _gameState.PlayerGameStates[senderPeerId].EndTurn();
    }
}