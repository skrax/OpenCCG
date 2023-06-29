using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class Card : TextureRect, INodeInit<CardGameStateDto>
{
    private static bool _canPreview = true;

    private CardGameStateDto _cardGameState;
    [Export] private PackedScene _cardPreviewScene;
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;
    [Export] private CardInfoPanel _infoPanel, _namePanel;
    private CardPreview? _preview;

    public Guid Id { get; private set; }

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