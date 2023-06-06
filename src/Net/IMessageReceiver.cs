using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace OpenCCG.Net;

public interface IMessageReceiver<in TMessageType>
{
    public Dictionary<string, IObserver>? Observers { get; }
    
    public MultiplayerApi Multiplayer { get; }
    
    public Error RpcId(long peerId, StringName method, params Variant[] args);
    
    public void RpcConfig(StringName method, Variant config);
    public void HandleMessage(string messageJson);

    Func<int, string?, string?> GetExecutor(TMessageType messageType);

    static void HandleMessage(IMessageReceiver<TMessageType> self, string messageJson)
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
            var responseData = executor.Invoke(senderPeerId, message.Json);

            if (responseData == null) return;

            var response = message with { Json = responseData };
            var responseJson = JsonSerializer.Serialize(response);

            self.RpcId(senderPeerId, nameof(HandleMessage), responseJson);
        }
    }

    static Func<int, string?, string?> MakeExecutor(Action act) => (_, _) =>
    {
        act();
        return null;
    };

    static Func<int, string?, string?> MakeExecutor(Action<int> act) => (senderPeerId, _) =>
    {
        act(senderPeerId);
        return null;
    };

    static Func<int, string?, string?> MakeExecutor<TIn>(Action<TIn> act) => (_, input) =>
    {
        act(JsonSerializer.Deserialize<TIn>(input!)!);
        return null;
    };

    static Func<int, string?, string?> MakeExecutor<TIn>(Action<int, TIn> act) => (senderPeerId, input) =>
    {
        act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
        return null;
    };

    static Func<int, string?, string?> MakeExecutor<TOut>(Func<TOut> act) => (_, _) =>
    {
        var response = act();
        return JsonSerializer.Serialize(response);
    };

    static Func<int, string?, string?> MakeExecutor<TOut>(Func<int, TOut> act) => (senderPeerId, _) =>
    {
        var response = act(senderPeerId);
        return JsonSerializer.Serialize(response);
    };

    static Func<int, string?, string?> MakeExecutor<TIn, TOut>(Func<int, TIn, TOut> act) => (senderPeerId, input) =>
    {
        var response = act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
        return JsonSerializer.Serialize(response);
    };


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