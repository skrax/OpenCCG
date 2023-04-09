using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class CardBoard : Sprite2D, INodeInit<CardGameStateDto>
{
    private CardStatPanel _atkPanel, _defPanel;
    private bool _isDragging, _canStopDrag;
    public CardGameStateDto CardGameState;

    public void Init(CardGameStateDto cardGameState)
    {
        CardGameState = cardGameState;
        _atkPanel.Value = CardGameState.Atk;
        _defPanel.Value = cardGameState.Def;

        Texture = GD.Load<Texture2D>(CardGameState.Record.ImgPath);
    }

    public void Update(CardGameStateDto cardGameState)
    {
        _atkPanel.Value = cardGameState.Atk;
        _defPanel.Value = cardGameState.Def;

        CardGameState = cardGameState;
    }

    public override void _Ready()
    {
        _atkPanel = GetChild<CardStatPanel>(0);
        _defPanel = GetChild<CardStatPanel>(1);
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
}