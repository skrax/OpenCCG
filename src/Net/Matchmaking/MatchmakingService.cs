using System.Collections.Generic;
using Godot;
using OpenCCG.Net.Messaging;
using Serilog;
using Error = OpenCCG.Net.Messaging.Error;
using IMessageBroker = OpenCCG.Net.Messaging.IMessageBroker;
using IMessageController = OpenCCG.Net.Messaging.IMessageController;
using MessageContext = OpenCCG.Net.Messaging.MessageContext;

namespace OpenCCG.Net.Matchmaking;

[GlobalClass]
public partial class MatchmakingService : Node, IMessageController
{
    private readonly MatchmakingQueue _defaultQueue = new();
    private readonly Dictionary<string, MatchmakingQueue> _passwordQueues = new();

    public void Configure(IMessageBroker broker)
    {
        broker.Map(Route.Enqueue, OnPlayerEnqueue);
        Log.Information("{Service} is running", nameof(MatchmakingService));
    }

    private MessageControllerResult OnPlayerEnqueue(MessageContext messageContext)
    {
        if (!messageContext.Message.TryUnwrap<MatchmakingRequest>(out var queuePlayerDto))
        {
            Log.Error("Failed to unwrap queue player dto");
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.BadRequest));
        }

        if (!TryGetQueue(queuePlayerDto, out var queue))
        {
            Log.Error("Failed to enqueue player {PeerId}", messageContext.PeerId);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.InternalServerError));
        }
        else
        {
            queue.Enqueue(messageContext.PeerId, queuePlayerDto);
            Log.Information("Player enqueued {PeerId}", messageContext.PeerId);

            return MessageControllerResult.AsResult();
        }
    }

    private bool TryGetQueue(MatchmakingRequest dto, out MatchmakingQueue queue)
    {
        if (dto.Password is not null)
        {
            return TryGetPasswordProtectedQueue(dto.Password, out queue);
        }

        queue = _defaultQueue;
        return true;
    }

    private bool TryGetPasswordProtectedQueue(string password, out MatchmakingQueue queue)
    {
        if (_passwordQueues.TryGetValue(password, out queue!))
        {
            return true;
        }

        var newQueue = new MatchmakingQueue();
        if (!_passwordQueues.TryAdd(password, newQueue))
        {
            return false;
        }

        queue = newQueue;
        return true;
    }

    public override void _Process(double delta)
    {
        while (_defaultQueue.TryGetPair(out var player1, out var player2))
        {
            Log.Information("Session created {Player1}, {Player2}", player1.PeerId, player2.PeerId);
        }

        foreach (var passwordQueue in _passwordQueues.Values)
        {
            while (passwordQueue.TryGetPair(out var player1, out var player2))
            {
                Log.Information("Session created {Player1}, {Player2}", player1.PeerId, player2.PeerId);
            }
        }
    }
}