using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class CardEffectPreview : TextureRect, IMessageReceiver<MessageType>
{
    [Export] private CardStatPanel _costPanel;
    public RequireTargetInputDto? CurrentInputDto;
    [Export] private CardInfoPanel _descriptionPanel, _namePanel;
    [Export] private SkipSelectionField _skipSelectionField;
    [Export] private Sprite2D _projectile;

    private TaskCompletionSource<RequireTargetOutputDto>? _tsc;

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor? GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.RequireTarget => Executor.Make<RequireTargetInputDto, RequireTargetOutputDto>(RequireTarget),
            MessageType.TmpShowCard => Executor.Make<CardImplementationDto>(TmpShowTarget,
                Executor.ResponseMode.NoResponse),
            _ => null
        };
    }

    private void Show(CardImplementationDto dto)
    {
        Visible = true;
        Texture = GD.Load<Texture2D>(dto.Outline.ImgPath);
        _descriptionPanel.Value = dto.Outline.Description;
        _namePanel.Value = dto.Outline.Name;
        _costPanel.Value = dto.State.Cost;
    }

    private void Reset()
    {
        Texture = null;
        Visible = false;
        CurrentInputDto = null;
    }

    private async Task<RequireTargetOutputDto> RequireTarget(RequireTargetInputDto input)
    {
        Show(input.Card);
        _tsc = new TaskCompletionSource<RequireTargetOutputDto>();
        CurrentInputDto = input;

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
        if (CurrentInputDto == null) return;

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
        EventSink.OnDragSelectTargetStart?.Invoke(CurrentInputDto!);

        ForceDrag(GetInstanceId(), preview);
    }

    private async Task TmpShowTarget(CardImplementationDto dto)
    {
        Show(dto);
        //await Task.Delay(TimeSpan.FromSeconds(3));
        _projectile.Visible = true;
        var start = GlobalPosition;
        var end = new Vector2(960F, 540F);
        var direction = end - start;
        var rot = Mathf.Atan2(direction.Y, direction.X) - 90f;
        Logger.Info($"facing: {rot},{Mathf.RadToDeg(rot)}");

        _projectile.Rotation = rot;
        var t = 0f;
        const float delta = 1000 / 60f;
        while (t < 1f)
        {
            t += delta / (360 * 2);
            t = Math.Clamp(t, 0f, 1f);

            var pos = new Vector2(
                Mathf.Lerp(start.X, end.X, t),
                Mathf.Lerp(start.Y, end.Y, t));

            _projectile.GlobalPosition = pos;
            await Task.Delay(TimeSpan.FromMilliseconds(delta));
        }

        _projectile.Visible = false;

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

        if (target is CardBoard cardBoard && CurrentInputDto!.Type != RequireTargetType.Avatar)
        {
            var isEnemyBoard = cardBoard.GetParent<BoardArea>().IsEnemy;
            if ((isEnemyBoard && CurrentInputDto.Side == RequireTargetSide.Friendly) ||
                (!isEnemyBoard && CurrentInputDto.Side == RequireTargetSide.Enemy))
            {
                ForceDrag();
                return;
            }

            Reset();
            if (!_tsc.TrySetResult(new RequireTargetOutputDto(cardBoard.CardImplementationDto.Id, null)))
            {
                ForceDrag();
                return;
            }
        }

        if (target is Avatar { IsEnemy: false } &&
            CurrentInputDto!.Type != RequireTargetType.Creature &&
            CurrentInputDto!.Side != RequireTargetSide.Enemy)
        {
            Reset();
            if (!_tsc.TrySetResult(new RequireTargetOutputDto(null, false)))
            {
                ForceDrag();
                return;
            }
        }

        if (target is Avatar { IsEnemy: true } &&
            CurrentInputDto!.Type != RequireTargetType.Creature &&
            CurrentInputDto.Side != RequireTargetSide.Friendly)
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