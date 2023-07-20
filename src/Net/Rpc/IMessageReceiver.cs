using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core.Serilog;
using Serilog;

namespace OpenCCG.Net.Rpc;

public interface IMessageReceiver<in TMessageType> : IGodotRpcNode
{
    /// <summary>
    ///     Required if your implementation uses <see cref="GetAsync{TIn,TOut}" />
    /// </summary>
    Dictionary<string, IObserver>? Observers { get; }

    /// <summary>
    ///     Rpc Method to receive messages.
    ///     Has to be annotated with <see cref="RpcAttribute" />.
    /// </summary>
    /// <param name="messageJson"></param>
    void HandleMessageAsync(string messageJson);

    /// <summary>
    ///     Provides an Executor for the specified MessageType
    /// </summary>
    /// <param name="messageType"></param>
    /// <returns></returns>
    Executor? GetExecutor(TMessageType messageType);

    /// <summary>
    ///     Call this from <see cref="HandleMessageAsync(string)" />.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="messageJson"></param>
    static async Task HandleMessageAsync(IMessageReceiver<TMessageType> self, string messageJson)
    {
        var senderPeerId = self.Multiplayer.GetRemoteSenderId();

        var logger = GetLogger(senderPeerId);

        var message = JsonSerializer.Deserialize<Message<TMessageType>>(messageJson)!;

        logger.Information("Receiving: {Message} with {MessageId} from {SenderPeerId}", message, message.Id,
            senderPeerId);

        if (self.Observers != null && self.Observers.TryGetValue(message.Id, out var observer))
        {
            logger.Information("Triggering Observer: {MessageId} from {SenderPeerId}", message.Id, senderPeerId);
            observer.Handle(message.Json);
            self.Observers.Remove(message.Id);
            observer.Dispose();
        }
        else
        {
            var executor = self.GetExecutor(message.Type);
            if (executor == null)
            {
                logger.Warning("No executor for {MessageId} from {SenderPeerId}", message.Id, senderPeerId);
            }
            else
            {
                var responseData = executor.Execution is Executor.ExecutionMode.Async
                    ? await executor.AsyncOp!(senderPeerId, message.Json)
                    : executor.Op!(senderPeerId, message.Json);

                if (executor.Response == Executor.ResponseMode.NoResponse) return;

                var response = message with { Json = responseData };
                var responseJson = JsonSerializer.Serialize(response);

                logger.Information("Responding: {Response} {MessageId} to {SenderPeerId}", response, response.Id,
                    senderPeerId);
                self.RpcId(senderPeerId, nameof(HandleMessageAsync), responseJson);
            }
        }
    }


    /// <summary>
    ///     Send a message to a peer and adds a new entry to <see cref="Observers" /> which awaits a response.
    /// </summary>
    /// <param name="self">Implementation</param>
    /// <param name="peerId">Peer to message</param>
    /// <param name="messageType">Type of message</param>
    /// <param name="input">Data to send</param>
    /// <param name="timeOut">TimeSpan until awaiting a response is canceled. Defaults to 3 minutes</param>
    /// <typeparam name="TIn">Type of sent data</typeparam>
    /// <typeparam name="TOut">Type of received data</typeparam>
    /// <returns></returns>
    static async Task<TOut> GetAsync<TIn, TOut>(IMessageReceiver<TMessageType> self, long peerId,
        TMessageType messageType, TIn input, TimeSpan? timeOut = null)
    {
        var logger = GetLogger(peerId);

        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType, JsonSerializer.Serialize(input));

        var tsc = new TaskCompletionSource<TOut>();
        timeOut ??= TimeSpan.FromMinutes(3);
        var observer = new Observer<TOut>(tsc, timeOut.Value);
        self.Observers!.Add(id, observer);

        logger.Information("Sending: {Message} with id {MessageId} to {PeerId} as {Method}", message, message.Id,
            peerId,
            nameof(GetAsync));
        self.RpcId(peerId, nameof(HandleMessageAsync), JsonSerializer.Serialize(message));

        return await tsc.Task;
    }


    /// <summary>
    ///     Send a message to a peer and adds a new entry to <see cref="Observers" /> which awaits a response.
    /// </summary>
    /// <param name="self">Implementation</param>
    /// <param name="peerId">Peer to message</param>
    /// <param name="messageType">Type of message</param>
    /// <param name="input">Data to send</param>
    /// <param name="timeOut">TimeSpan until awaiting a response is canceled. Defaults to 3 minutes</param>
    /// <typeparam name="TIn">Type of sent data</typeparam>
    /// <typeparam name="TOut">Type of received data</typeparam>
    /// <returns></returns>
    static async Task GetAsync<TIn>(IMessageReceiver<TMessageType> self, long peerId,
        TMessageType messageType, TIn input, TimeSpan? timeOut = null)
    {
        var logger = GetLogger(peerId);

        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType, JsonSerializer.Serialize(input));

        var tsc = new TaskCompletionSource();
        timeOut ??= TimeSpan.FromMinutes(3);
        var observer = new Observer(tsc, timeOut.Value);
        self.Observers!.Add(id, observer);

        logger.Information("Sending: {Message} with id {MessageId} to {PeerId} as {Method}", message, message.Id,
            peerId,
            nameof(GetAsync));
        self.RpcId(peerId, nameof(HandleMessageAsync), JsonSerializer.Serialize(message));

        await tsc.Task;
    }


    /// <summary>
    ///     Send a message to a peer and adds a new entry to <see cref="Observers" /> which awaits a response.
    /// </summary>
    /// <param name="self">Implementation</param>
    /// <param name="peerId">Peer to message</param>
    /// <param name="messageType">Type of message</param>
    /// <param name="timeOut">TimeSpan until awaiting a response is canceled. Defaults to 3 minutes</param>
    /// <returns></returns>
    static async Task GetAsync(IMessageReceiver<TMessageType> self, long peerId,
        TMessageType messageType, TimeSpan? timeOut = null)
    {
        var logger = GetLogger(peerId);

        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType);

        var tsc = new TaskCompletionSource();
        timeOut ??= TimeSpan.FromMinutes(3);
        var observer = new Observer(tsc, timeOut.Value);
        self.Observers!.Add(id, observer);

        logger.Information("Sending: {Message} with id {MessageId} to {PeerId} as {Method}", message, message.Id,
            peerId,
            nameof(GetAsync));
        self.RpcId(peerId, nameof(HandleMessageAsync), JsonSerializer.Serialize(message));

        await tsc.Task;
    }

    /// <summary>
    ///     Sends a message to a peer without awaiting a response.
    /// </summary>
    /// <param name="self">Implementation</param>
    /// <param name="peerId">Peer to message</param>
    /// <param name="messageType">Type of message</param>
    static void FireAndForget(IMessageReceiver<TMessageType> self, long peerId, TMessageType messageType)
    {
        var logger = GetLogger(peerId);

        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType);

        logger.Information("Sending: {Message} with id {MessageId} to {PeerId} as {Method}", message, message.Id,
            peerId,
            nameof(FireAndForget));
        self.RpcId(peerId, nameof(HandleMessageAsync), JsonSerializer.Serialize(message));
    }

    /// <summary>
    ///     Sends a message to a peer without awaiting a response
    /// </summary>
    /// <param name="self">Implementation</param>
    /// <param name="peerId">Peer to message</param>
    /// <param name="messageType">Type of message</param>
    /// <param name="t">Data to send</param>
    /// <typeparam name="T">Type of data to send</typeparam>
    static void FireAndForget<T>(IMessageReceiver<TMessageType> self, long peerId, TMessageType messageType, T t)
    {
        var logger = GetLogger(peerId);

        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType, JsonSerializer.Serialize(t));
        logger.Information("Sending: {Message} with id {MessageId} to {PeerId} as {Method}", message, message.Id,
            peerId,
            nameof(FireAndForget));
        self.RpcId(peerId, nameof(HandleMessageAsync), JsonSerializer.Serialize(message));
    }

    public static ILogger GetLogger(long peerId) =>
        SessionContextProvider.SessionContextsByPeerId.TryGetValue(peerId, out var sessionContext)
            ? Log.ForContext(new SessionContextEnricher(sessionContext))
            : Log.Logger;
}