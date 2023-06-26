using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public record QueuePlayerDto(CardUIDeck.JsonRecord[] Deck, string? Password);

public record QueuedPlayer(long peerId, List<CardRecord> deckList);

public partial class Server : Node, IMessageReceiver<MessageType>
{
    private readonly GameState _gameState = new();
    private RpcNodes _rpcNodes;
    private readonly Dictionary<string, QueuedPlayer> _queuesByPassword = new();

    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateServer(57618, 16);

        if (result is Error.Ok)
        {
            Logger.Info<Server>("listening on port 57618");
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

        _rpcNodes = new RpcNodes
        {
            Server = this,
            Hand = GetNode<Hand>("Hand"),
            EnemyHand = GetNode<EnemyHand>("EnemyHand"),
            Board = GetNode<Board>("Board"),
            EnemyBoard = GetNode<Board>("EnemyBoard"),
            StatusPanel = GetNode<ServerNodes.StatusPanel>("StatusPanel"),
            EnemyStatusPanel = GetNode<ServerNodes.StatusPanel>("EnemyStatusPanel"),
            MidPanel = GetNode<ServerNodes.MidPanel>("MidPanel"),
            CardEffectPreview = GetNode<ServerNodes.CardEffectPreview>("CardEffectPreview"),
            EnemyCardEffectPreview = GetNode<ServerNodes.CardEffectPreview>("EnemyCardEffectPreview")
        };
    }

    private void OnPeerConnected(long id)
    {
        Logger.Info<Server>($"Peer connected {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Info<Server>($"Peer disconnected {id}");
        if (_gameState.PlayerGameStates.TryGetValue(id, out var playerGameState))
        {
            playerGameState.Disconnect();
            _gameState.PlayerGameStates.Remove(id);
            _gameState.PlayerGameStates.Remove(playerGameState.EnemyPeerId);
        }
    }

    private async Task<bool> PlayCard(long senderPeerId, Guid cardId)
    {
        return await _gameState.PlayerGameStates[senderPeerId].PlayCardAsync(cardId);
    }

    private async Task CombatPlayerCard(long senderPeerId, CombatPlayerCardDto t)
    {
        await _gameState.PlayerGameStates[senderPeerId].CombatAsync(t.AttackerId, t.TargetId);
    }

    private async Task CombatPlayer(long senderPeerId, Guid cardId)
    {
        await _gameState.PlayerGameStates[senderPeerId].CombatPlayerAsync(cardId);
    }

    private async Task EndTurn(long senderPeerId)
    {
        await _gameState.PlayerGameStates[senderPeerId].EndTurnAsync();
    }

    private async Task QueuePlayer(long senderPeerId, QueuePlayerDto queuePlayerDto)
    {
        var deckList = new List<CardRecord>(30);
        foreach (var card in queuePlayerDto.Deck)
        {
            var cardRecord = Database.Cards[card.Id];
            for (var i = 0; i < card.Count; ++i) deckList.Add(cardRecord);
        }

        var key = queuePlayerDto.Password ?? string.Empty;

        if (_queuesByPassword.TryGetValue(key, out var other))
        {
            if (!Multiplayer.GetPeers().Contains((int)other.peerId))
            {
                _queuesByPassword[key] = new QueuedPlayer(senderPeerId, deckList);
                return;
            }

            _queuesByPassword.Remove(key);

            var p1 = new PlayerGameState
            {
                PeerId = other.peerId,
                EnemyPeerId = senderPeerId,
                PlayerName = "player-1",
                Nodes = _rpcNodes
            };
            var p2 = new PlayerGameState
            {
                PeerId = senderPeerId,
                EnemyPeerId = other.peerId,
                PlayerName = "player-2",
                Nodes = _rpcNodes
            };

            p1.Init(other.deckList);
            p2.Init(deckList);

            p1.Enemy = p2;
            p2.Enemy = p1;

            _gameState.PlayerGameStates.Add(p1.PeerId, p1);
            _gameState.PlayerGameStates.Add(p2.PeerId, p2);

            p1.Start();
            p2.Start();
            await p1.StartTurnAsync();
        }
        else
        {
            _queuesByPassword.Add(key, new QueuedPlayer(senderPeerId, deckList));
            _rpcNodes.MidPanel.SetStatusMessage(senderPeerId, "Looking for opponent");
            _rpcNodes.MidPanel.EndTurnButtonSetActive(senderPeerId, new EndTurnButtonSetActiveDto(false, ""));
        }
    }

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
            MessageType.PlayCard => Executor.Make<Guid, bool>(PlayCard),
            MessageType.CombatPlayerCard => Executor.Make<CombatPlayerCardDto>(CombatPlayerCard),
            MessageType.CombatPlayer => Executor.Make<Guid>(CombatPlayer),
            MessageType.EndTurn => Executor.Make(EndTurn, Executor.ResponseMode.NoResponse),
            MessageType.Queue => Executor.Make<QueuePlayerDto>(QueuePlayer),
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
        };
    }
}