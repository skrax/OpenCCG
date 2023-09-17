using System;
using System.Text.Json;

namespace OpenCCG.Net.Messaging;

public record Message(Guid Id, string Route, string? Data, Error? Error, string? ResponseRoute, Guid? ResponseId)
{
    public static Message Create(string route) => new(Guid.NewGuid(), route, null, null, null, null);

    public static Message CreateWithResponse(string route, string responseRoute) =>
        new(Guid.NewGuid(), route, null, null, responseRoute, Guid.NewGuid());

    public static Message Create<TIn>(string route, TIn data) =>
        new(Guid.NewGuid(), route, JsonSerializer.Serialize(data), null, null, null);

    public static Message CreateWithResponse<TIn>(string route, string responseRoute, TIn data) =>
        new(Guid.NewGuid(), route, JsonSerializer.Serialize(data), null, responseRoute, Guid.NewGuid());

    public static Message CreateError(string route, Error error) => new(Guid.NewGuid(), route, null, error, null, null);

    public Message ToResponse() =>
        new(ResponseId!.Value, ResponseRoute!, null, null, null, null);

    public Message ToResponse(object data, Type dataType) =>
        new(ResponseId!.Value, ResponseRoute!, JsonSerializer.Serialize(value: data, inputType: dataType),
            null, null, null);

    public Message ToErrorResponse(Error error) => new(ResponseId!.Value, ResponseRoute!, null, error, null, null);


    public TIn? Unwrap<TIn>()
    {
        if (typeof(TIn) == typeof(string)) throw new ArgumentException("Do not unwrap strings");
        return Data is null
            ? default
            : JsonSerializer.Deserialize<TIn>(Data);
    }

    public bool TryUnwrap<TIn>(out TIn result)
    {
        var unwrapped = Unwrap<TIn>();
        if (unwrapped is not null)
        {
            result = unwrapped;
            return true;
        }

        result = default!;
        return false;
    }

    public bool TryGetError(out Error error)
    {
        if (HasError())
        {
            error = Error!;
            return true;
        }

        error = null!;
        return false;
    }

    public bool HasResponseInformation() => ResponseId.HasValue && !string.IsNullOrWhiteSpace(ResponseRoute);

    public bool HasError() => Error is not null;
    public bool HasData() => Data is not null;
}