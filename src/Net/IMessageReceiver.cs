using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace OpenCCG.Net;

public interface IMessageReceiver<in TMessageType>
{
    Dictionary<string, IObserver>? Observers { get; }

    MultiplayerApi Multiplayer { get; }

    Error RpcId(long peerId, StringName method, params Variant[] args);

    void HandleMessage(string messageJson);

    Executor GetExecutor(TMessageType messageType);

    static async Task HandleMessage(IMessageReceiver<TMessageType> self, string messageJson)
    {
        var senderPeerId = self.Multiplayer.GetRemoteSenderId();

        var message = JsonSerializer.Deserialize<Message<TMessageType>>(messageJson)!;

        if (self.Observers != null && self.Observers.TryGetValue(message.Id, out var observer))
        {
            observer.Handle(message.Json);
            self.Observers.Remove(message.Id);
            observer.Dispose();
        }
        else
        {
            var executor = self.GetExecutor(message.Type);
            var responseData = executor.IsAsync
                ? await executor.AsyncOp!(senderPeerId, message.Json)
                : executor.Op!(senderPeerId, message.Json);

            if (responseData == null) return;

            var response = message with { Json = responseData };
            var responseJson = JsonSerializer.Serialize(response);

            self.RpcId(senderPeerId, nameof(HandleMessage), responseJson);
        }
    }

    static Executor MakeExecutor(Action act) => Executor.Make((_, _) =>
    {
        act();
        return null;
    });

    static Executor MakeExecutor(Action<int> act) => Executor.Make((senderPeerId, _) =>
    {
        act(senderPeerId);
        return null;
    });

    static Executor MakeExecutor<TIn>(Action<TIn> act) => Executor.Make((_, input) =>
    {
        act(JsonSerializer.Deserialize<TIn>(input!)!);
        return null;
    });


    static Executor MakeExecutor<TIn>(Action<int, TIn> act) => Executor.Make((senderPeerId, input) =>
    {
        act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
        return null;
    });

    static Executor MakeExecutor<TOut>(Func<TOut> act) => Executor.Make((_, _) =>
    {
        var response = act();
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor<TOut>(Func<int, TOut> act) => Executor.Make((senderPeerId, _) =>
    {
        var response = act(senderPeerId);
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor<TIn, TOut>(Func<TIn, TOut> act) => Executor.Make((_, input) =>
    {
        var response = act(JsonSerializer.Deserialize<TIn>(input!)!);
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor<TIn, TOut>(Func<int, TIn, TOut> act) => Executor.Make((senderPeerId, input) =>
    {
        var response = act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor(Func<Task> act) => Executor.MakeAsync(async (_, _) =>
    {
        await act();
        return null;
    });

    static Executor MakeExecutor(Func<int, Task> act) => Executor.MakeAsync(async (senderPeerId, _) =>
    {
        await act(senderPeerId);
        return null;
    });

    static Executor MakeExecutor<TIn>(Func<TIn, Task> act) => Executor.MakeAsync(async (_, input) =>
    {
        await act(JsonSerializer.Deserialize<TIn>(input!)!);
        return null;
    });

    static Executor MakeExecutor<TIn>(Func<int, TIn, Task> act) => Executor.MakeAsync(async (senderPeerId, input) =>
    {
        await act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
        return null;
    });

    static Executor MakeExecutor<TOut>(Func<Task<TOut>> act) => Executor.MakeAsync(async (_, _) =>
    {
        var response = await act();
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor<TOut>(Func<int, Task<TOut>> act) => Executor.MakeAsync(async (senderPeerId, _) =>
    {
        var response = await act(senderPeerId);
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor<TIn, TOut>(Func<TIn, Task<TOut>> act) => Executor.MakeAsync(async (_, input) =>
    {
        var response = await act(JsonSerializer.Deserialize<TIn>(input!)!);
        return JsonSerializer.Serialize(response);
    });

    static Executor MakeExecutor<TIn, TOut>(Func<int, TIn, Task<TOut>> act) => Executor.MakeAsync(
        async (senderPeerId, input) =>
        {
            var response = await act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        });

    static async Task<TOut> GetAsync<TIn, TOut>(IMessageReceiver<TMessageType> self, long peerId,
        TMessageType messageType, TIn input)
    {
        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType, JsonSerializer.Serialize(input));

        var tsc = new TaskCompletionSource<TOut>();
        var observer = new Observer<TOut>(tsc);
        self.Observers!.Add(id, observer);

        self.RpcId(peerId, nameof(HandleMessage), JsonSerializer.Serialize(message));

        return await tsc.Task;
    }


    static void FireAndForget(IMessageReceiver<TMessageType> self, long peerId, TMessageType messageType)
    {
        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType);

        self.RpcId(peerId, nameof(HandleMessage), JsonSerializer.Serialize(message));
    }

    static void FireAndForget<T>(IMessageReceiver<TMessageType> self, long peerId, TMessageType messageType, T t)
    {
        var id = Guid.NewGuid().ToString();
        var message = new Message<TMessageType>(id, messageType, JsonSerializer.Serialize(t));

        self.RpcId(peerId, nameof(HandleMessage), JsonSerializer.Serialize(message));
    }
}