using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG;

public partial class Card : TextureRect
{
    private static bool _canPreview = true;

    [Export] private PackedScene _cardPreviewScene = null!;
    [Export] private CardStatPanel _costPanel = null!, _atkPanel = null!, _defPanel = null!;
    [Export] private Curve _drawCurve = null!;
    [Export] private CardInfoPanel _infoPanel = null!, _namePanel = null!;

    private MessageBroker _broker = null!;
    private CardImplementationDto _cardGameState = null!;
    private CardPreview? _preview;
    public Guid Id { get; private set; }

    public void Init(CardImplementationDto dto, MessageBroker broker)
    {
        SetProcess(false);
        _cardGameState = dto;
        _broker = broker;
        Id = dto.Id;

        _infoPanel.Value = _cardGameState.Outline.Description;
        _namePanel.Value = _cardGameState.Outline.Name;
        Texture = GD.Load<Texture2D>(_cardGameState.Outline.ImgPath);
        _costPanel.SetValue(_cardGameState.State.Cost, _cardGameState.Outline.Cost);

        if (_cardGameState.IsCreature)
        {
            _atkPanel.SetValue(_cardGameState.CreatureState!.Atk, _cardGameState.CreatureOutline!.Atk);
            _defPanel.SetValue(_cardGameState.CreatureState!.Def, _cardGameState.CreatureOutline!.Def);
        }
        else if (_cardGameState.IsSpell)
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

    public async void Play()
    {
        var response = await _broker.EnqueueMessageAndGetResponseAsync(1, Message.CreateWithResponse(
            Route.PlayCard, Route.PlayCardResponse, Id));

        if (response is null || response.Message.HasError())
        {
            Log.Error("failed to play card");
        }
        else
        {
            Log.Information("play card");
        }
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", true);

        _preview?.DrawOutline(enabled);
    }
}