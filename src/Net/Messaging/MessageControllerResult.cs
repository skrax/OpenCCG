using System;

namespace OpenCCG.Net.Messaging;

public record MessageControllerResult(object? Data, Type? DataType, Error? Error)
{
    public static MessageControllerResult? AsDeferred() => null;
    public static MessageControllerResult AsResult() => new(null, null, null);
    public static MessageControllerResult AsResult<T>(T data) where T : class => new(data, typeof(T), null);

    public static MessageControllerResult AsError(Error error) => new(null, null, error);
    public bool IsSuccess() => Error is null;
    public bool IsError() => Error is not null;
    public bool HasData() => Data is not null && DataType is not null;
}