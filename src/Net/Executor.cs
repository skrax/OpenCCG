using System;
using System.Threading.Tasks;

namespace OpenCCG.Net;

public record Executor(bool IsAsync, Func<int, string?, string?>? Op, Func<int, string?, Task<string?>>? AsyncOp)
{
    public static Executor Make(Func<int, string?, string?> op) => new(false, op, null);
    public static Executor MakeAsync(Func<int, string?, Task<string?>> op) => new(true, null, op);
}