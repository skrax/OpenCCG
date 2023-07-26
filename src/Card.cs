using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Card : TextureRect, INodeInit<CardImplementationDto>
{
    private static bool _canPreview = true;

    private CardImplementationDto _cardGameState = null!;
    [Export] private PackedScene _cardPreviewScene = null!;
    [Export] private CardStatPanel _costPanel = null!, _atkPanel = null!, _defPanel = null!;
    [Export] private Curve _drawCurve = null!;
    [Export] private CardInfoPanel _infoPanel = null!, _namePanel = null!;
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
        _costPanel.SetValue(dto.State.Cost, dto.Outline.Cost);

        if (dto.IsCreature)
        {
            _atkPanel.SetValue(dto.CreatureState!.Atk, dto.CreatureOutline!.Atk);
            _defPanel.SetValue(dto.CreatureState!.Def, dto.CreatureOutline!.Def);
        }
        else if (dto.IsSpell)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }

        MouseEntered += ShowPreview;
        MouseExited += DisablePreview;

        Modulate = Colors.Transparent;
    }

    public async Task PlayDrawAnimAsync(Vector2 start, Vector2 end)
    {
        Modulate = Colors.White;
        var t = 0f;
        const float delta = 1000 / 60f;
        while (t < 1f)
        {
            t += delta / 360;
            t = Math.Clamp(t, 0f, 1f);

            var pos = GlobalPosition;
            pos.X = Mathf.Lerp(start.X, end.X, t);
            var lol = (pos.X - end.X) / (start.X - end.X);

            var h = _drawCurve.Sample(lol) * -200f;
            pos.Y = start.Y + h;

            GlobalPosition = pos;
            await Task.Delay(TimeSpan.FromMilliseconds(delta));
        }
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