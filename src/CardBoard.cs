using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class CardBoard : Sprite2D, INodeInit<CardGameStateDto>
{
    [Export] private CardStatPanel _atkPanel, _defPanel;
    [Export] private Panel _dmgPopup;
    [Export] private AnimationPlayer _anim;
    private bool _isDragging, _canStopDrag;
    public CardGameStateDto CardGameState;
    public bool IsEnemy { get; private set; }

    public void Init(CardGameStateDto record)
    {
        IsEnemy = GetParent<BoardArea>().IsEnemy;
        CardGameState = record;
        _atkPanel.Value = CardGameState.Atk;
        _defPanel.Value = record.Def;

        Texture = GD.Load<Texture2D>(CardGameState.Record.ImgPath);

        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("doMix", record.ISummoningProtectionOn);
        if (!IsEnemy)
        {
            var canAttack = record is { IsSummoningSicknessOn: false, AttacksAvailable: > 0 };
            shader?.SetShaderParameter("drawOutline", canAttack);
        }
    }

    public void Destroy(Action act)
    {
        _anim.AnimationFinished += _ =>
        {
            QueueFree();
            act();
        };
        _anim.Play("shake");
    }

    public async Task UpdateAsync(CardGameStateDto cardGameState)
    {
        _atkPanel.Value = cardGameState.Atk;
        var diff = cardGameState.Def - _defPanel.Value;
        _defPanel.Value = cardGameState.Def;

        CardGameState = cardGameState;

        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("doMix", cardGameState.ISummoningProtectionOn);
        if (!IsEnemy)
        {
            var canAttack = cardGameState is { IsSummoningSicknessOn: false, AttacksAvailable: > 0 };
            shader?.SetShaderParameter("drawOutline", canAttack);
        }

        if (diff != 0)
        {
            _dmgPopup.GetChild<Label>(0).Text = diff > 0 ? $"+ {diff}" : $"{diff}";
            _dmgPopup.Visible = true;
            await Task.Delay(TimeSpan.FromSeconds(2));
            _dmgPopup.Visible = false;
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(InputActions.SpriteClick))
        {
            var rect = GetRect();
            var inputEventMouseButton = (InputEventMouseButton)inputEvent;

            if (!rect.HasPoint(ToLocal(inputEventMouseButton.Position))) return;

            EventSink.ReportPointerDown(this);
        }

        if (inputEvent.IsActionReleased(InputActions.SpriteClick))
        {
            var rect = GetRect();
            var inputEventMouseButton = (InputEventMouseButton)inputEvent;

            if (!rect.HasPoint(ToLocal(inputEventMouseButton.Position))) return;

            EventSink.ReportPointerUp(this);
        }
    }

    public async Task AttackAsync(Sprite2D sprite2D)
    {
        var f = 0f;
        var targetPosition = sprite2D.GlobalPosition;
        var off = sprite2D.GetRect().Size.Y * sprite2D.Scale.Y * 0.9f;
        if (sprite2D is Avatar { IsEnemy: true } or CardBoard { IsEnemy: true })
        {
            targetPosition.Y += off;
        }
        else
        {
            targetPosition.Y -= off;
        }

        var oldPosition = GlobalPosition;
        ZIndex = 1;

        while (f < 0.7f)
        {
            f += 0.012800001F;
            GlobalPosition = GlobalPosition.Lerp(targetPosition, f);
            await Task.Delay(TimeSpan.FromMilliseconds(16D));
        }

        GlobalPosition = oldPosition;
        ZIndex = 0;
    }
}