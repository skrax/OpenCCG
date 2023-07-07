using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Card : TextureRect, INodeInit<CardImplementationDto>
{
    private static bool _canPreview = true;

    private CardImplementationDto _cardGameState;
    [Export] private PackedScene _cardPreviewScene;
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;
    [Export] private CardInfoPanel _infoPanel, _namePanel;
    private CardPreview? _preview;

    public Guid Id { get; private set; }

    public void Init(CardImplementationDto dto)
    {
        SetProcess(false);
        _cardGameState = dto;
        Id = dto.Id;
        _infoPanel.Value = dto.Outline.Description;
        _namePanel.Value = dto.Outline.Name;
        Texture = GD.Load<Texture2D>(dto.Outline.ImgPath);
        _costPanel.Value = dto.State.Cost;
        
        if (dto.IsCreature)
        {
            _atkPanel.Value = dto.CreatureState!.Atk;
            _defPanel.Value = dto.CreatureState!.Def;
        }
        else if (dto.IsSpell)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }

        MouseEntered += ShowPreview;
        MouseExited += DisablePreview;
    }

    public override void _Process(double delta)
    {
        Visible = true;
        SetProcess(false);
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        MouseEntered -= ShowPreview;
        MouseExited -= DisablePreview;
        DisablePreview();
        var preview = new Control();
        var duplicate = Duplicate() as Card;
        preview.AddChild(duplicate);
        duplicate!.Position = -duplicate.Size / 2;
        SetDragPreview(preview);
        preview.TreeExited += () =>
        {
            SetProcess(true);
            _canPreview = true;
            MouseEntered += ShowPreview;
            MouseExited += DisablePreview;
            EventSink.OnDragCardStop?.Invoke();
        };
        Visible = false;
        _canPreview = false;

        EventSink.OnDragCardStart?.Invoke();

        return GetInstanceId();
    }

    public void ShowPreview()
    {
        if (!_canPreview) return;

        Modulate = Colors.Transparent;
        _preview ??= _cardPreviewScene.Make<CardPreview>(GetParent().GetParent());
        _preview.Init(_cardGameState);
        var pos = GlobalPosition;
        pos.Y -= Size.Y + 40;
        pos.X -= _preview.Size.X / 2 - Size.X / 2;
        _preview.Position = pos;
        _preview.Visible = true;
        _preview.ZIndex = ZIndex + 1;
    }

    public void DisablePreview()
    {
        Modulate = Colors.White;

        if (_preview == null) return;
        _preview.Visible = false;
    }

    public async Task PlayAsync()
    {
        Visible = false;
        var success = await GetNode<Main>("/root/Main").TryPlayCardAsync(Id);
        if (!success) Visible = true;
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", true);

        _preview?.DrawOutline(enabled);
    }
}