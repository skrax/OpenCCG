using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCCG.Net;

public class Observer<T> : IObserver
{
    private readonly TaskCompletionSource<T> _tsc;
    private readonly Timer _timer;
    private bool _handled;

    public Observer(TaskCompletionSource<T> tsc) : this(tsc, TimeSpan.FromMinutes(3))
    {
    }

    public Observer(TaskCompletionSource<T> tsc, TimeSpan timeOut)
    {
        _tsc = tsc;
        _timer = new Timer(_ =>
            {
                if (_handled) return;
                _tsc.TrySetCanceled();
            },
            null,
            timeOut,
            Timeout.InfiniteTimeSpan
        );
    }

    public void Handle(string json)
    {
        _handled = true;
        if (_tsc.Task.IsCanceled) return;
        var response = JsonSerializer.Deserialize<T>(json)!;
        _tsc.TrySetResult(response);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _timer.Dispose();
    }
}