using System.Linq;
using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class InputEventSystem : Node2D
{
    public enum InputState
    {
        Idle,
        DraggingCard,
        DraggingLine
    }

    private InputState _state = InputState.Idle;
    private Line2D _line;
    private Vector2 _dragOffset;
    private Card? _cardToDrag;

    public override void _Ready()
    {
        _line = GetChild<Line2D>(0);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(InputActions.SpriteClick))
        {
            if (_state is InputState.Idle)
            {
                var cardBoard = EventSink.PointerDownCardBoard.MinBy(x => x.ZIndex);
                if (cardBoard != null)
                {
                    var mousePosition = ((InputEventMouseButton)inputEvent).Position;
                    _state = InputState.DraggingLine;
                    Logger.Info<InputEventSystem>($"DragLine Start {cardBoard.GetInstanceId()}");
                    _line.Points = new[] { _line.ToLocal(mousePosition), _line.ToLocal(mousePosition) };
                }

                var card = EventSink.PointerDownCards.MinBy(x => x.ZIndex);
                if (card != null)
                {
                    var mousePosition = ((InputEventMouseButton)inputEvent).Position;
                    _state = InputState.DraggingCard;
                    _dragOffset = mousePosition - card.Position;
                    _cardToDrag = card;
                    card.ZIndex = 1;
                }
            }
        }

        if (inputEvent.IsActionReleased(InputActions.SpriteClick))
        {
            switch (_state)
            {
                case InputState.DraggingLine:
                {
                    var cardBoard = EventSink.PointerUpCardBoard.MinBy(x => x.ZIndex);
                    if (cardBoard != null)
                    {
                        Logger.Info<InputEventSystem>($"DragLine End {cardBoard.GetInstanceId()}");
                    }

                    _line.Points = System.Array.Empty<Vector2>();
                    _state = InputState.Idle;
                    break;
                }
                case InputState.DraggingCard:
                    _cardToDrag!.ZIndex = 0;
                    _cardToDrag!.PlayOrInvokeDragFailure();
                    _cardToDrag = null;
                    _dragOffset = Vector2.Zero;
                    _state = InputState.Idle;
                    break;
            }
        }

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            switch (_state)
            {
                case InputState.DraggingLine:
                {
                    var mousePosition = mouseMotion.Position;
                    var first = _line.Points[0];
                    _line.Points = new[] { first, _line.ToLocal(mousePosition) };
                    break;
                }
                case InputState.DraggingCard:
                    _cardToDrag!.Position = mouseMotion.Position - _dragOffset;
                    break;
            }
        }


        EventSink.Drain();
    }
}