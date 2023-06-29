using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class CardEffectPreview : TextureRect, IMessageReceiver<MessageType>
{
    [Export] private CardStatPanel _costPanel;
    private RequireTargetInputDto? _currentInputDto;
    [Export] private CardInfoPanel _descriptionPanel, _namePanel;
    [Export] private SkipSelectionField _skipSelectionField;

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
            MessageType.RequireTarget => Executor.Make<RequireTargetInputDto, RequireTargetOutputDto>(RequireTarget),
            MessageType.TmpShowCard => Executor.Make<CardGameStateDto>(TmpShowTarget, Executor.ResponseMode.NoResponse)
        };
    }

    private void Show(CardGameStateDto cardGameStateDto)
    {
        Visible = true;
        Texture = GD.Load<Texture2D>(cardGameStateDto.Record.ImgPath);
        _costPanel.Value = cardGameStateDto.Cost;
        _descriptionPanel.Value = cardGameStateDto.Record.Description;
        _namePanel.Value = cardGameStateDto.Record.Name;
    }

    private void Reset()
    {
        Texture = null;
        Visible = false;
        _currentInputDto = null;
    }

    private async Task<RequireTargetOutputDto> RequireTarget(RequireTargetInputDto input)
    {
        Show(input.Card);
        _tsc = new TaskCompletionSource<RequireTargetOutputDto>();
        _currentInputDto = input;

        ForceDrag();

        return await _tsc.Task;
    }

    public override void _Process(double delta)
    {
        SetProcess(false);
        if (_tsc is null || _tsc.Task.IsCompleted || _tsc.Task.IsCanceled || _tsc.Task.IsCanceled ||
            _tsc.Task.IsFaulted)
            return;

        ForceDrag();
    }

    private void ForceDrag()
    {
        if (_currentInputDto == null) return;

        var line = GetNode<TargetLine>("/root/Main/TargetLine");
        var preview = new Control();
        preview.GlobalPosition = GetGlobalMousePosition();
        preview.TreeExited += () =>
        {
            EventSink.OnDragSelectTargetStop?.Invoke();
            _skipSelectionField.Disable();
            line.Reset();
            SetProcess(true);
        };
        line.Target(this, preview);
        _skipSelectionField.Enable();
        EventSink.OnDragSelectTargetStart?.Invoke(_currentInputDto!);

        ForceDrag(GetInstanceId(), preview);
    }

    private async Task TmpShowTarget(CardGameStateDto cardGameStateDto)
    {
        Show(cardGameStateDto);
        await Task.Delay(TimeSpan.FromSeconds(3));
        Reset();
    }

    public void SkipTarget()
    {
        if (_tsc is null || _tsc.Task.IsCompleted || _tsc.Task.IsCanceled || _tsc.Task.IsCanceled ||
            _tsc.Task.IsFaulted)
        {
            Logger.Error<CardEffectPreview>($"No request id set to use {nameof(SkipTarget)}");
            ForceDrag();
            return;
        }

        Reset();
        _skipSelectionField.Disable();

        if (!_tsc.TrySetResult(new RequireTargetOutputDto(null, null))) ForceDrag();
    }

    public void TryUpstreamTarget<T>(T target)
    {
        if (_tsc is null || _tsc.Task.IsCompleted || _tsc.Task.IsCanceled || _tsc.Task.IsCanceled ||
            _tsc.Task.IsFaulted)
        {
            Logger.Error<CardEffectPreview>($"No request id set to use {nameof(TryUpstreamTarget)}");
            ForceDrag();
            return;
        }


        _skipSelectionField.Disable();

        if (target is CardBoard cardBoard && _currentInputDto!.Type != RequireTargetType.Avatar)
        {
            var isEnemyBoard = cardBoard.GetParent<BoardArea>().IsEnemy;
            if ((isEnemyBoard && _currentInputDto.Side == RequireTargetSide.Friendly) ||
                (!isEnemyBoard && _currentInputDto.Side == RequireTargetSide.Enemy))
            {
                ForceDrag();
                return;
            }

            Reset();
            if (!_tsc.TrySetResult(new RequireTargetOutputDto(cardBoard.CardGameState.Id, null)))
            {
                ForceDrag();
                return;
            }
        }

        if (target is Avatar { IsEnemy: false } &&
            _currentInputDto!.Type != RequireTargetType.Creature &&
            _currentInputDto!.Side != RequireTargetSide.Enemy)
        {
            Reset();
            if (!_tsc.TrySetResult(new RequireTargetOutputDto(null, false)))
            {
                ForceDrag();
                return;
            }
        }

        if (target is Avatar { IsEnemy: true } &&
            _currentInputDto!.Type != RequireTargetType.Creature &&
            _currentInputDto.Side != RequireTargetSide.Friendly)
        {
            Reset();
            if (!_tsc.TrySetResult(new RequireTargetOutputDto(null, true)))
            {
                ForceDrag();
                return;
            }
        }

        ForceDrag();
    }
}