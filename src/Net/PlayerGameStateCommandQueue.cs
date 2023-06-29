using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCCG.Core;
using OpenCCG.Net.Dto;

namespace OpenCCG.Net;

public class PlayerGameStateCommandQueue
{
    public readonly PlayerGameState PlayerGameState;
    private readonly DefaultBackgroundTaskQueue _taskQueue = new(100);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public PlayerGameStateCommandQueue(PlayerGameState playerGameState)
    {
        PlayerGameState = playerGameState;
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                Logger.Error<PlayerGameState>(ex, "Error occurred executing task work item.");
            }
        }
    }

    public void Start()
    {
        PlayerGameState.Start();
        Task.Run(() => ProcessTaskQueueAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public async Task<TaskCompletionSource> EnqueueWithCompletionAsync(Func<PlayerGameState, Func<Task>> selector)
    {
        var tsc = new TaskCompletionSource();
        await _taskQueue.QueueBackgroundWorkItemAsync(async _ =>
        {
            await selector(PlayerGameState)();
            tsc.SetResult();
        });

        return tsc;
    }

    public async Task<TaskCompletionSource> EnqueueWithCompletionAsync<T>(Func<PlayerGameState, Func<T, Task>> selector,
        T input)
    {
        var tsc = new TaskCompletionSource();
        await _taskQueue.QueueBackgroundWorkItemAsync(async _ =>
        {
            await selector(PlayerGameState)(input);
            tsc.SetResult();
        });

        return tsc;
    }

    public async Task<TaskCompletionSource<T>> EnqueueWithCompletionAsync<T>(
        Func<PlayerGameState, Func<Task<T>>> selector)
    {
        var tsc = new TaskCompletionSource<T>();
        await _taskQueue.QueueBackgroundWorkItemAsync(async _ =>
        {
            var result = await selector(PlayerGameState)();
            tsc.SetResult(result);
        });

        return tsc;
    }

    public async Task<TaskCompletionSource<TOut>> EnqueueWithCompletionAsync<TIn, TOut>(
        Func<PlayerGameState, Func<TIn, Task<TOut>>> selector,
        TIn input)
    {
        var tsc = new TaskCompletionSource<TOut>();
        await _taskQueue.QueueBackgroundWorkItemAsync(async _ =>
        {
            var result = await selector(PlayerGameState)(input);
            tsc.SetResult(result);
        });

        return tsc;
    }

    public async Task EnqueueAsync(Func<PlayerGameState, Action> selector)
    {
        await _taskQueue.QueueBackgroundWorkItemAsync(_ =>
        {
            selector(PlayerGameState)();
            return ValueTask.CompletedTask;
        });
    }

    public async Task EnqueueAsync(Func<PlayerGameState, Func<Task>> selector)
    {
        await _taskQueue.QueueBackgroundWorkItemAsync(async _ => { await selector(PlayerGameState)(); });
    }

    public async Task EnqueueAsync<T>(Func<PlayerGameState, Func<T, Task>> selector, T input)
    {
        await _taskQueue.QueueBackgroundWorkItemAsync(async _ => { await selector(PlayerGameState)(input); });
    }
}