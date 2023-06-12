using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class CardTempArea : Sprite2D, IMessageReceiver<MessageType>
{
    [Export] private InputEventSystem _inputEventSystem;

    private TaskCompletionSource<RequireTargetOutputDto>? _tsc;

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.RequireTarget => Executor.Make<RequireTargetInputDto, RequireTargetOutputDto>(RequireTarget)
        };
    }

    public void ShowPermanent(string imgPath)
    {
        Texture = GD.Load<Texture2D>(imgPath);
    }

    public async void Show(string imgPath, string timeSpan)
    {
        Texture = GD.Load<Texture2D>(imgPath);
        await Task.Delay(TimeSpan.Parse(timeSpan));
        Reset();
    }

    public void Reset()
    {
        Texture = null;
    }

    public async Task<RequireTargetOutputDto> RequireTarget(RequireTargetInputDto input)
    {
        ShowPermanent(input.ImgPath);
        _tsc = new TaskCompletionSource<RequireTargetOutputDto>();
        _inputEventSystem.OnRequireTarget();

        return await _tsc.Task;
    }

    public bool TryUpstreamTarget<T>(T target)
    {
        if (_tsc is null || _tsc.Task.IsCompleted || _tsc.Task.IsCanceled || _tsc.Task.IsCanceled ||
            _tsc.Task.IsFaulted)
        {
            Logger.Error<CardTempArea>($"No request id set to use {nameof(TryUpstreamTarget)}");
            return false;
        }

        if (target is CardBoard cardBoard)
        {
            Reset();
            return _tsc.TrySetResult(new RequireTargetOutputDto(cardBoard.CardGameState.Id));
        }

        if (target is EnemyAvatar)
        {
            Reset();
            return _tsc.TrySetResult(new RequireTargetOutputDto(null));
        }

        return false;
    }
}