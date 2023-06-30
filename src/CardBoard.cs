using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Dto;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class CardBoard : Control, INodeInit<CardGameStateDto>
{
    [Export] private AnimationPlayer _anim;
    [Export] private CardStatPanel _atkPanel, _defPanel;
    [Export] private PackedScene _cardPreviewScene;
    [Export] private Panel _dmgPopup;
    private bool _isDragging, _canStopDrag;
    private CardPreview? _preview;

    private bool _targetingDisabled;
    [Export] private TextureRect _textureRect;
    public CardGameStateDto CardGameState;
    public bool IsEnemy { get; private set; }

    public void Init(CardGameStateDto outline)
    {
        IsEnemy = GetParent<BoardArea>().IsEnemy;
        CardGameState = outline;
        _atkPanel.Value = CardGameState.Atk;
        _defPanel.Value = outline.Def;

        _textureRect.Texture = GD.Load<Texture2D>(CardGameState.Record.ImgPath);

        var shader = _textureRect.Material as ShaderMaterial;
        shader?.SetShaderParameter("doMix", outline.ISummoningProtectionOn);
        if (!IsEnemy)
        {
            var canAttack = outline is { IsSummoningSicknessOn: false, AttacksAvailable: > 0 };
            shader?.SetShaderParameter("drawOutline", canAttack);
        }

        MouseEntered += ShowPreview;
        MouseExited += DisablePreview;
        EventSink.OnDragForCombatStart += OnDragForCombatStart;
        EventSink.OnDragForCombatStop += OnDragForCombatStop;
        EventSink.OnDragSelectTargetStart += OnDragSelectTargetStart;
        EventSink.OnDragSelectTargetStop += OnDragSelectTargetStop;
    }

    public override void _ExitTree()
    {
        EventSink.OnDragForCombatStart -= OnDragForCombatStart;
        EventSink.OnDragForCombatStop -= OnDragForCombatStop;
        EventSink.OnDragSelectTargetStart -= OnDragSelectTargetStart;
        EventSink.OnDragSelectTargetStop -= OnDragSelectTargetStop;
    }

    private void OnDragSelectTargetStart(RequireTargetInputDto dto)
    {
        if (dto.Type == RequireTargetType.Avatar) return;
        if (!IsEnemy && dto.Side == RequireTargetSide.Enemy) return;
        if (IsEnemy && dto.Side == RequireTargetSide.Friendly) return;

        DrawOutline(true);
    }

    private void OnDragSelectTargetStop()
    {
        var canAttack = !IsEnemy && CardGameState is { IsSummoningSicknessOn: false, AttacksAvailable: > 0 };
        DrawOutline(canAttack);
    }

    private void OnDragForCombatStart(ulong instanceId)
    {
        if (GetInstanceId() == instanceId) return;
        if (!IsEnemy) return;
        if (CardGameState.ISummoningProtectionOn) return;
        if (!CardGameState.Record.Abilities.Defender &&
            GetParent<BoardArea>()._cards.Any(x => x.CardGameState.Record.Abilities.Defender)) return;

        DrawOutline(true);
    }

    private void OnDragForCombatStop(ulong instanceId)
    {
        if (GetInstanceId() == instanceId) return;
        if (!IsEnemy) return;

        DrawOutline(false);
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
        _targetingDisabled = true;

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
            await Task.Delay(TimeSpan.FromSeconds(1.5));
            _dmgPopup.Visible = false;
        }

        _targetingDisabled = false;
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
        preview.TreeExiting += () =>
        {
            line.Reset();
            EventSink.OnDragForCombatStop?.Invoke(GetInstanceId());
        };
        line.Target(this, preview);

        SetDragPreview(preview);
        EventSink.OnDragForCombatStart?.Invoke(GetInstanceId());
        return GetInstanceId();
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var instanceId = data.As<ulong>();
        if (instanceId == GetInstanceId()) return false;
        var obj = InstanceFromId(data.As<ulong>());

        return obj switch
        {
            CardBoard => IsEnemy && !CardGameState.ISummoningProtectionOn,
            CardEffectPreview => true,
            _ => false
        };
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var obj = InstanceFromId(data.As<ulong>());

        switch (obj)
        {
            case CardBoard attacker:
            {
                Logger.Info<CardBoard>($"{attacker!.CardGameState.Id} attacked {CardGameState.Id}");
                GetNode<Main>("/root/Main").CombatPlayerCard(attacker.CardGameState.Id, CardGameState.Id);
                break;
            }
            case CardEffectPreview effect:
            {
                effect.TryUpstreamTarget(this);
                break;
            }
        }
    }

    public async Task AttackAsync(Control sprite2D)
    {
        var f = 0f;
        var targetPosition = sprite2D.GlobalPosition;
        var off = sprite2D.GetRect().Size.Y * sprite2D.Scale.Y * 0.9f;
        if (sprite2D is Avatar { IsEnemy: true } or CardBoard { IsEnemy: true })
            targetPosition.Y += off;
        else
            targetPosition.Y -= off;

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

    private void DrawOutline(bool enabled)
    {
        var shader = _textureRect.Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", enabled);
    }
}