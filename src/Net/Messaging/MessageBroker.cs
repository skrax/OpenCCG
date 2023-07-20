using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using Serilog;

namespace OpenCCG.Net.Messaging;

public delegate MessageControllerResult? MessageResolver(MessageContext context);

public abstract partial class MessageBroker : Node, IMessageBroker
{
    private readonly Dictionary<string, MessageResolver> _routes = new();
    private readonly HashSet<Guid> _awaitedMessageIds = new();
    private readonly Dictionary<Guid, TaskCompletionSource<MessageContext?>> _awaitedMessagesByIds = new();
    private GlobalMessenger _globalMessenger = null!;

    private readonly Queue<MessageContext> _queuedMessages = new();

    public override void _EnterTree()
    {
        _globalMessenger = this.GetAutoloaded<GlobalMessenger>();
    }

    public override void _Process(double _)
    {
        while (_queuedMessages.TryDequeue(out var queuedMessage))
        {
            var result = _globalMessenger.SendMessage(queuedMessage.PeerId, queuedMessage.Message);
            Log.Information("Sent message {Route} to {PeerId} with result {Result}",
                queuedMessage.Message.Route,
                queuedMessage.PeerId, result);
        }
    }

    protected ENetMultiplayerPeer? CreateServer(int port, int maxClients)
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateServer(port, maxClients);

        switch (result)
        {
            case Godot.Error.Ok:
                Log.Information("listening on port {Port}", port);
                GetTree().GetMultiplayer().MultiplayerPeer = peer;
                Multiplayer.PeerConnected += OnPeerConnected;
                Multiplayer.PeerDisconnected += OnPeerDisconnected;

                _globalMessenger.OnReceived += HandleMessage;

                return peer;
            case Godot.Error.AlreadyInUse:
                peer.Close();
                return null;
            case Godot.Error.CantCreate:
            default:
                throw new ApplicationException("Failed to create server");
        }
    }

    protected ENetMultiplayerPeer? CreateClient(string serverAddress, int port)
    {
        var peer = new ENetMultiplayerPeer();
        var result = peer.CreateClient(serverAddress, port);

        switch (result)
        {
            case Godot.Error.Ok:
            {
                var mp = GetTree().GetMultiplayer();
                mp.MultiplayerPeer = peer;

                Multiplayer.ConnectedToServer += OnConnectedToServer;
                Multiplayer.ServerDisconnected += OnServerDisconnected;
                Multiplayer.PeerConnected += OnPeerConnected;
                Multiplayer.PeerDisconnected += OnPeerDisconnected;
                Multiplayer.ConnectionFailed += OnConnectionFailed;

                _globalMessenger.OnReceived += HandleMessage;

                return peer;
            }
            case Godot.Error.AlreadyInUse:
                peer.Close();
                return null;
            case Godot.Error.CantCreate:
            default:
                throw new ApplicationException("Failed to connect to server");
        }
    }

    private void HandleMessage(Message message)
    {
        if (_routes.TryGetValue(message.Route, out var resolver))
        {
            var peerId = Multiplayer.GetRemoteSenderId();
            var result = resolver.Invoke(new(peerId, message));
            if (result is not null)
            {
                if (result.IsError())
                {
                    RespondError(peerId, message, result.Error!);
                }
                else if (result.IsSuccess())
                {
                    if (result.HasData())
                    {
                        Respond(peerId, message, result.Data, result.DataType);
                    }
                    else
                    {
                        Respond(peerId, message);
                    }
                }
            }
        }
        else
        {
            Log.Warning("No route {Route} found", message.Route);
        }
    }

    private void Respond(long peerId, Message message)
    {
        if (!message.HasResponseInformation()) return;

        var response = message.ToResponse();
        EnqueueMessage(peerId, response);
    }

    private void Respond(long peerId, Message message, object data, Type dataType)
    {
        if (!message.HasResponseInformation()) return;

        var response = message.ToResponse(data, dataType);
        EnqueueMessage(peerId, response);
    }

    private void RespondError(long peerId, Message message, Error error)
    {
        if (!message.HasResponseInformation()) return;

        var response = message.ToErrorResponse(error);
        EnqueueMessage(peerId, response);
    }

    private void MapInternal(string route, MessageResolver resolver)
    {
        if (!_routes.TryAdd(route, resolver))
        {
            Log.Error("Route {Route} already exists", route);
        }
    }

    public void Map(string route, MessageResolver resolver)
    {
        MapInternal(route, resolver);
    }

    public void MapResponseForTask(string route)
    {
        MapInternal(route, ctx =>
        {
            if (!_awaitedMessagesByIds.Remove(ctx.Message.Id, out var tsc))
            {
                Log.Error("Received message with unknown id {Id}", ctx.Message.Id);
                return null;
            }

            tsc.SetResult(ctx);
            return null;
        });
    }

    public void MapResponse(string route, MessageResolver resolver)
    {
        MapInternal(route, ctx =>
        {
            var dataJson = ctx.Message.Data;
            if (dataJson == null)
            {
                Log.Error("No data for {Route} received", route);
                return null;
            }

            if (!_awaitedMessageIds.Remove(ctx.Message.Id))
            {
                Log.Error("Received message with unknown id {Id}", ctx.Message.Id);
                return null;
            }

            return resolver(ctx);
        });
    }

    public void EnqueueMessage(long peerId, Message message)
    {
        _queuedMessages.Enqueue(new MessageContext(peerId, message));
    }

    public bool TryEnqueueMessageWithResponse(long peerId, Message message)
    {
        if (!message.ResponseId.HasValue || _awaitedMessageIds.Contains(message.ResponseId.Value))
        {
            Log.Error("Cannot enqueue message with duplicate id {Id}", message.Id);
            return false;
        }

        _awaitedMessageIds.Add(message.ResponseId.Value);
        EnqueueMessage(peerId, message);

        return true;
    }

    public Task<MessageContext?> EnqueueMessageAndGetResponseAsync(long peerId, Message message)
        => EnqueueMessageAndGetResponseAsync(peerId, message, TimeSpan.FromSeconds(3));

    public Task<MessageContext?> EnqueueMessageAndGetResponseAsync(long peerId, Message message, TimeSpan timeout)
    {
        if (!message.ResponseId.HasValue || _awaitedMessagesByIds.ContainsKey(message.ResponseId.Value))
        {
            Log.Error("Cannot enqueue message with duplicate id {Id}", message.Id);
            return Task.FromResult<MessageContext?>(null);
        }

        var tsc = new TaskCompletionSource<MessageContext?>();

        _awaitedMessagesByIds.Add(message.ResponseId.Value, tsc);
        EnqueueMessage(peerId, message);

        var timer = GetTree().CreateTimer(timeout.TotalSeconds);
        timer.Timeout += () => { tsc.TrySetCanceled(); };

        return tsc.Task;
    }

    public override void _ExitTree()
    {
        _globalMessenger.OnReceived -= HandleMessage;
    }

    protected virtual void OnConnectionFailed()
    {
    }

    protected virtual void OnServerDisconnected()
    {
    }

    protected virtual void OnConnectedToServer()
    {
    }

    protected virtual void OnPeerConnected(long id)
    {
    }

    protected virtual void OnPeerDisconnected(long id)
    {
    }
}