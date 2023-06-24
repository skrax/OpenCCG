using System;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class Card : TextureRect, INodeInit<CardGameStateDto>
{
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;
    [Export] private CardInfoPanel _infoPanel, _namePanel;
    [Export] private PackedScene _cardPreviewScene;

    private CardGameStateDto _cardGameState;
    private CardPreview? _preview;
    private static bool _canPreview = true;

    public Guid Id { get; private set; }

    public override void _Process(double delta)
    {
        Visible = true;
        SetProcess(false);
    }

    public void Init(CardGameStateDto card)
    {
        SetProcess(false);
        _cardGameState = card;
        var record = card.Record;
        Id = card.Id;
        _infoPanel.Value = record.Description;
        _costPanel.Value = card.Cost;
        _atkPanel.Value = card.Atk;
        _defPanel.Value = card.Def;
        _namePanel.Value = record.Name;
        if (record.Type is not CardRecordType.Creature)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }

        Logger.Info<Card>($"Init Card: {card.Atk}/{card.Def}/{card.Cost}");
        Texture = GD.Load<Texture2D>(record.ImgPath);
        MouseEntered += ShowPreview;
        MouseExited += DisablePreview;
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
        };
        Visible = false;
        _canPreview = false;

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
    }

    public void DisablePreview()
    {
        Modulate = Colors.White;
        
        if (_preview == null) return;
        _preview.Visible = false;
    }

    public void Play()
    {
        Visible = false;
        GetNode<Main>("/root/Main").PlayCard(Id);
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", true);

        _preview?.DrawOutline(enabled);
    }
}