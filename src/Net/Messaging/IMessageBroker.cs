using System;
using System.Threading.Tasks;

namespace OpenCCG.Net.Messaging;

public record MessageControllerResult(object? Data, Type? DataType, Error? Error)
{
    public static MessageControllerResult AsResult() => new(null, null, null);
    public static MessageControllerResult AsResult<T>(T data) where T : class => new(data, typeof(T), null);

    public static MessageControllerResult AsError(Error error) => new(null, null, error);
    public bool IsSuccess() => Error is null;
    public bool IsError() => Error is not null;
    public bool HasData() => Data is not null && DataType is not null;
}

public interface IMessageBroker
{
    public void Map(string route, MessageResolver resolver);

    public void MapResponseForTask(string route);

    public void MapResponse(string route, MessageResolver resolver);

    public void EnqueueMessage(long peerId, Message message);

    public bool TryEnqueueMessageWithResponse(long peerId, Message message);

    public Task<MessageContext?> EnqueueMessageAndGetResponseAsync(long peerId, Message message);
    public Task<MessageContext?> EnqueueMessageAndGetResponseAsync(long peerId, Message message, TimeSpan timeout);
}