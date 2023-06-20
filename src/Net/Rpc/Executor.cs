using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenCCG.Net.Rpc;

public record Executor(Executor.ExecutionMode Execution, Executor.ResponseMode Response, Func<long, string?, string?>? Op,
    Func<long, string?, Task<string?>>? AsyncOp)
{
    public enum ExecutionMode
    {
        Sync,
        Async
    }

    public enum ResponseMode
    {
        NoResponse,
        Respond
    }

    private static Executor MakeOp(Func<long, string?, string?> op, ResponseMode isFireAndForget)
    {
        return new Executor(ExecutionMode.Sync, isFireAndForget, op, null);
    }

    private static Executor MakeAsyncOp(Func<long, string?, Task<string?>> op, ResponseMode isFireAndForget)
    {
        return new Executor(ExecutionMode.Async, isFireAndForget, null, op);
    }

    public static Executor Make(Action act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeOp((_, _) =>
        {
            act();
            return null;
        }, isFireAndForget);
    }

    public static Executor Make(Action<long> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeOp((senderPeerId, _) =>
        {
            act(senderPeerId);
            return null;
        }, isFireAndForget);
    }

    public static Executor Make<TIn>(Action<TIn> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeOp((_, input) =>
        {
            act(JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        }, isFireAndForget);
    }


    public static Executor Make<TIn>(Action<long, TIn> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeOp((senderPeerId, input) =>
        {
            act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        }, isFireAndForget);
    }

    public static Executor Make<TOut>(Func<TOut> act)
    {
        return MakeOp((_, _) =>
        {
            var response = act();
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make<TOut>(Func<long, TOut> act)
    {
        return MakeOp((senderPeerId, _) =>
        {
            var response = act(senderPeerId);
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make<TIn, TOut>(Func<TIn, TOut> act)
    {
        return MakeOp((_, input) =>
        {
            var response = act(JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make<TIn, TOut>(Func<long, TIn, TOut> act)
    {
        return MakeOp((senderPeerId, input) =>
        {
            var response = act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make(Func<Task> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeAsyncOp(async (_, _) =>
        {
            await act();
            return null;
        }, isFireAndForget);
    }

    public static Executor Make(Func<long, Task> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeAsyncOp(async (senderPeerId, _) =>
        {
            await act(senderPeerId);
            return null;
        }, isFireAndForget);
    }

    public static Executor Make<TIn>(Func<TIn, Task> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeAsyncOp(async (_, input) =>
        {
            await act(JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        }, isFireAndForget);
    }

    public static Executor Make<TIn>(Func<long, TIn, Task> act, ResponseMode isFireAndForget = ResponseMode.NoResponse)
    {
        return MakeAsyncOp(async (senderPeerId, input) =>
        {
            await act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        }, isFireAndForget);
    }

    public static Executor Make<TOut>(Func<Task<TOut>> act)
    {
        return MakeAsyncOp(async (_, _) =>
        {
            var response = await act();
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make<TOut>(Func<long, Task<TOut>> act)
    {
        return MakeAsyncOp(async (senderPeerId, _) =>
        {
            var response = await act(senderPeerId);
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make<TIn, TOut>(Func<TIn, Task<TOut>> act)
    {
        return MakeAsyncOp(async (_, input) =>
        {
            var response = await act(JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }

    public static Executor Make<TIn, TOut>(Func<long, TIn, Task<TOut>> act)
    {
        return MakeAsyncOp(async (senderPeerId, input) =>
        {
            var response = await act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        }, ResponseMode.Respond);
    }
}