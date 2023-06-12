using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenCCG.Net.Rpc;

public record Executor(bool IsAsync, Func<int, string?, string?>? Op, Func<int, string?, Task<string?>>? AsyncOp)
{
    private static Executor MakeOp(Func<int, string?, string?> op)
    {
        return new Executor(false, op, null);
    }

    private static Executor MakeAsyncOp(Func<int, string?, Task<string?>> op)
    {
        return new Executor(true, null, op);
    }

    public static Executor Make(Action act)
    {
        return MakeOp((_, _) =>
        {
            act();
            return null;
        });
    }

    public static Executor Make(Action<int> act)
    {
        return MakeOp((senderPeerId, _) =>
        {
            act(senderPeerId);
            return null;
        });
    }

    public static Executor Make<TIn>(Action<TIn> act)
    {
        return MakeOp((_, input) =>
        {
            act(JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        });
    }


    public static Executor Make<TIn>(Action<int, TIn> act)
    {
        return MakeOp((senderPeerId, input) =>
        {
            act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        });
    }

    public static Executor Make<TOut>(Func<TOut> act)
    {
        return MakeOp((_, _) =>
        {
            var response = act();
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make<TOut>(Func<int, TOut> act)
    {
        return MakeOp((senderPeerId, _) =>
        {
            var response = act(senderPeerId);
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make<TIn, TOut>(Func<TIn, TOut> act)
    {
        return MakeOp((_, input) =>
        {
            var response = act(JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make<TIn, TOut>(Func<int, TIn, TOut> act)
    {
        return MakeOp((senderPeerId, input) =>
        {
            var response = act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make(Func<Task> act)
    {
        return MakeAsyncOp(async (_, _) =>
        {
            await act();
            return null;
        });
    }

    public static Executor Make(Func<int, Task> act)
    {
        return MakeAsyncOp(async (senderPeerId, _) =>
        {
            await act(senderPeerId);
            return null;
        });
    }

    public static Executor Make<TIn>(Func<TIn, Task> act)
    {
        return MakeAsyncOp(async (_, input) =>
        {
            await act(JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        });
    }

    public static Executor Make<TIn>(Func<int, TIn, Task> act)
    {
        return MakeAsyncOp(async (senderPeerId, input) =>
        {
            await act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
            return null;
        });
    }

    public static Executor Make<TOut>(Func<Task<TOut>> act)
    {
        return MakeAsyncOp(async (_, _) =>
        {
            var response = await act();
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make<TOut>(Func<int, Task<TOut>> act)
    {
        return MakeAsyncOp(async (senderPeerId, _) =>
        {
            var response = await act(senderPeerId);
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make<TIn, TOut>(Func<TIn, Task<TOut>> act)
    {
        return MakeAsyncOp(async (_, input) =>
        {
            var response = await act(JsonSerializer.Deserialize<TIn>(input!)!);
            return JsonSerializer.Serialize(response);
        });
    }

    public static Executor Make<TIn, TOut>(Func<int, TIn, Task<TOut>> act)
    {
        return MakeAsyncOp(
            async (senderPeerId, input) =>
            {
                var response = await act(senderPeerId, JsonSerializer.Deserialize<TIn>(input!)!);
                return JsonSerializer.Serialize(response);
            });
    }
}