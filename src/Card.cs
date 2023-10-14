using Godot;

namespace OpenCCG;

public partial class Card : TextureRect
{
    #if false
    private static bool _canPreview = true;

    [Export] private PackedScene _cardPreviewScene = null!;
    [Export] private CardStatPanel _costPanel = null!, _atkPanel = null!, _defPanel = null!;
    [Export] private Curve _drawCurve = null!;
    [Export] private CardInfoPanel _infoPanel = null!, _namePanel = null!;

    private ProtobufPeerTransport _transport = null!;
    private CardImplementationDto _cardGameState = null!;
    private CardPreview? _preview;
    public Guid Id { get; private set; }

    public void Init(CardImplementationDto dto, ProtobufPeerTransport transport)
    {
        SetProcess(false);
        _cardGameState = dto;
        _transport = transport;
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
        var response = await _transport.PutAsync("cards/playById", new ById
        {
            Id = Id.ToString()
        });

        if (response is not GenericResponse genericResponse)
        {
            Log.Error("Cannot handle response from server");
        }
        else
        {
            switch (genericResponse.StatusCode)
            {
                case StatusCode.Ok:
                    Log.Information("Played card");
                    break;
                case StatusCode.Forbidden:
                case StatusCode.BadRequest:
                    Log.Error("Failed to play card {StatusCode}",genericResponse.StatusCode.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", true);

        _preview?.DrawOutline(enabled);
    }
#endif
}