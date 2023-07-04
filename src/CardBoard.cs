using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Net.Dto;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class CardBoard : Control, INodeInit<CardImplementationDto>
{
    [Export] private AnimationPlayer _anim;
    [Export] private CardStatPanel _atkPanel, _defPanel;
    [Export] private PackedScene _cardPreviewScene;
    [Export] private Panel _dmgPopup;
    private bool _isDragging, _canStopDrag;
    private CardPreview? _preview;

    [Export] private TextureRect _textureRect;
    public CardImplementationDto CardImplementationDto;
    public bool IsEnemy { get; private set; }

    public void Init(CardImplementationDto dto)
    {
        IsEnemy = GetParent<BoardArea>().IsEnemy;
        CardImplementationDto = dto;
        _atkPanel.Value = CardImplementationDto.CreatureState!.Atk;
        _defPanel.Value = dto.CreatureState!.Def;

        _textureRect.Texture = GD.Load<Texture2D>(CardImplementationDto.Outline.ImgPath);

        var shader = _textureRect.Material as ShaderMaterial;
        shader?.SetShaderParameter("doMix", !dto.CreatureState!.IsExposed);
        if (!IsEnemy)
        {
            var canAttack = dto.CreatureState is { AttacksAvailable: > 0 };
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
        if (dto.Card.IsCreature && !dto.Card.CreatureAbilities!.Arcane &&
            !CardImplementationDto.CreatureState!.IsExposed) return;

        DrawOutline(true);
    }

    private void OnDragSelectTargetStop()
    {
        var canAttack = !IsEnemy && CardImplementationDto.CreatureState is { AttacksAvailable: > 0 };
        DrawOutline(canAttack);
    }

    private void OnDragForCombatStart(ulong instanceId)
    {
        if (GetInstanceId() == instanceId) return;
        if (!IsEnemy) return;
        if (!CardImplementationDto.CreatureState!.IsExposed) return;
        if (!CardImplementationDto.CreatureAbilities!.Defender &&
            GetParent<BoardArea>()._cards.Any(x => x.CardImplementationDto.CreatureAbilities!.Defender)) return;

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

    public async Task UpdateAsync(CardImplementationDto dto)
    {
        CardImplementationDto = dto;
        _atkPanel.Value = dto.CreatureState!.Atk;
        var diff = dto.CreatureState!.Def - _defPanel.Value;
        _defPanel.Value = dto.CreatureState!.Def;


        var shader = _textureRect.Material as ShaderMaterial;
        shader?.SetShaderParameter("doMix", !dto.CreatureState!.IsExposed);
        if (!IsEnemy)
        {
            var canAttack = dto.CreatureState is { AttacksAvailable: > 0 };
            shader?.SetShaderParameter("drawOutline", canAttack);
        }

        if (diff != 0)
        {
            _dmgPopup.GetChild<Label>(0).Text = diff > 0 ? $"+ {diff}" : $"{diff}";
            _dmgPopup.Visible = true;
            await Task.Delay(TimeSpan.FromSeconds(1.5));
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
        if (CardImplementationDto.CreatureState!.AttacksAvailable <= 0) return default;
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

        switch (obj)
        {
            case CardBoard:
                return IsEnemy && CardImplementationDto.CreatureState!.IsExposed;
            case CardEffectPreview preview:
            {
                if (preview.CurrentInputDto!.Side == RequireTargetSide.Enemy && !IsEnemy) return false;
                if (preview.CurrentInputDto!.Side == RequireTargetSide.Friendly && IsEnemy) return false;
                if (preview.CurrentInputDto.Card.IsCreature &&
                    !preview.CurrentInputDto.Card.CreatureAbilities!.Arcane &&
                    !CardImplementationDto.CreatureState!.IsExposed)
                    return false;

                return true;
            }
            default:
                return false;
        }
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var obj = InstanceFromId(data.As<ulong>());

        switch (obj)
        {
            case CardBoard attacker:
            {
                Logger.Info<CardBoard>($"{attacker!.CardImplementationDto.Id} attacked {CardImplementationDto.Id}");
                GetNode<Main>("/root/Main")
                    .CombatPlayerCard(attacker.CardImplementationDto.Id, CardImplementationDto.Id);
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
        _preview.Init(CardImplementationDto);
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