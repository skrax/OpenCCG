using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class CardBoard : Control, INodeInit<CardGameStateDto>
{
    [Export] private TextureRect _textureRect;
    [Export] private CardStatPanel _atkPanel, _defPanel;
    [Export] private Panel _dmgPopup;
    [Export] private AnimationPlayer _anim;
    private bool _isDragging, _canStopDrag;
    public CardGameStateDto CardGameState;
    public bool IsEnemy { get; private set; }

    private bool _hovering, _canHover = true;
    [Export] private PackedScene _cardPreviewScene;
    private CardPreview? _preview;

    public void Init(CardGameStateDto record)
    {
        IsEnemy = GetParent<BoardArea>().IsEnemy;
        CardGameState = record;
        _atkPanel.Value = CardGameState.Atk;
        _defPanel.Value = record.Def;

        _textureRect.Texture = GD.Load<Texture2D>(CardGameState.Record.ImgPath);

        var shader = _textureRect.Material as ShaderMaterial;
        shader?.SetShaderParameter("doMix", record.ISummoningProtectionOn);
        if (!IsEnemy)
        {
            var canAttack = record is { IsSummoningSicknessOn: false, AttacksAvailable: > 0 };
            shader?.SetShaderParameter("drawOutline", canAttack);
        }
        
        MouseEntered += ShowPreview;
        MouseExited += DisablePreview;
    }

    public void Destroy(Action act)
    {
        _anim.AnimationFinished += _ =>
        {
            _preview?.QueueFree();
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

        var shader = _textureRect.Material as ShaderMaterial;
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

    public void ForceDrag()
    {
        var line = GetNode<TargetLine>("/root/Main/TargetLine");
        var preview = new Control();
        preview.TreeExiting += () => { line.Reset(); };
        line.Target(this, preview);

        ForceDrag(GetInstanceId(), preview);
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (CardGameState.AttacksAvailable <= 0) return default;
        if (CardGameState.IsSummoningSicknessOn) return default;
        if (IsEnemy) return default;
        
        var line = GetNode<TargetLine>("/root/Main/TargetLine");
        var preview = new Control();
        preview.TreeExiting += () => { line.Reset(); };
        line.Target(this, preview);

        SetDragPreview(preview);
        return GetInstanceId();
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var instanceId = data.As<ulong>();
        return instanceId != GetInstanceId() &&
               IsEnemy &&
               !CardGameState.ISummoningProtectionOn &&
               InstanceFromId(data.As<ulong>()) is CardBoard;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var attacker = InstanceFromId(data.As<ulong>()) as CardBoard;

        Logger.Info<CardBoard>($"{attacker!.CardGameState.Id} attacked {CardGameState.Id}");
        GetNode<Main>("/root/Main").CombatPlayerCard(attacker.CardGameState.Id, CardGameState.Id);
    }

    public async Task AttackAsync(Control sprite2D)
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

    private void ShowPreview()
    {
        _preview ??= _cardPreviewScene.Make<CardPreview>(GetParent().GetParent());
        _preview.Init(CardGameState);
        var pos = GlobalPosition;
        pos.X += Size.X + 40;
        pos.Y -= _preview.Size.Y / 2 - Size.Y / 2;
        _preview.GlobalPosition = pos;
        _preview.Visible = true;
    }

    private void DisablePreview()
    {
        if (IsQueuedForDeletion() || _preview == null) return;
        _preview.Visible = false;
    }
}