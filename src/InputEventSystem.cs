using System;
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
    private ulong? _dragLineStartInstanceId;


    public override void _Ready()
    {
        _line = GetChild<Line2D>(0);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(InputActions.SpriteClick))
        {
            OnSpriteClickedPressed(inputEvent);
        }

        if (inputEvent.IsActionReleased(InputActions.SpriteClick))
        {
            OnSpriteClickReleased();
        }

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            OnMouseMotion(mouseMotion);
        }

        EventSink.Drain();
    }

    private void OnSpriteClickReleased()
    {
        switch (_state)
        {
            case InputState.DraggingLine:
            {
                OnDragLineEnd();
                break;
            }
            case InputState.DraggingCard:
            {
                OnDragCardEnd();
                break;
            }
            case InputState.Idle:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnSpriteClickedPressed(InputEvent inputEvent)
    {
        if (_state is not InputState.Idle) return;

        var mousePosition = ((InputEventMouseButton)inputEvent).Position;
        var cardBoard = EventSink.PointerDownCardBoard.MinBy(x => x.ZIndex);
        if (cardBoard != null)
        {
            if (cardBoard.GetParentOrNull<BoardArea>() != null)
                OnDragLineStart(cardBoard, mousePosition);
            return;
        }

        var card = EventSink.PointerDownCards.MinBy(x => x.ZIndex);
        if (card != null)
        {
            OnDragCardStart(card, mousePosition);
        }
    }

    private void OnMouseMotion(InputEventMouseMotion mouseMotion)
    {
        switch (_state)
        {
            case InputState.DraggingLine:
            {
                OnDragLineUpdate(mouseMotion);
                break;
            }
            case InputState.DraggingCard:
            {
                OnDragCardUpdate(mouseMotion);
                break;
            }
            case InputState.Idle:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnDragCardUpdate(InputEventMouseMotion mouseMotion)
    {
        _cardToDrag!.Position = mouseMotion.Position - _dragOffset;
    }

    private void OnDragLineUpdate(InputEventMouseMotion mouseMotion)
    {
        var mousePosition = mouseMotion.Position;
        var first = _line.Points[0];
        _line.Points = new[] { first, _line.ToLocal(mousePosition) };
    }

    private void OnDragLineStart(CardBoard cardBoard, Vector2 mousePosition)
    {
        _state = InputState.DraggingLine;
        Logger.Info<InputEventSystem>($"DragLine Start {cardBoard.GetInstanceId()}");
        _line.Points = new[] { _line.ToLocal(mousePosition), _line.ToLocal(mousePosition) };
        _dragLineStartInstanceId = cardBoard.GetInstanceId();
    }

    private void OnDragLineEnd()
    {
        var cardBoard = EventSink.PointerUpCardBoard.MinBy(x => x.ZIndex);
        Logger.Info<InputEventSystem>($"DragLine End {cardBoard?.GetInstanceId()}");

        _line.Points = Array.Empty<Vector2>();
        _state = InputState.Idle;

        if (cardBoard != null && _dragLineStartInstanceId.HasValue)
        {
            if (InstanceFromId(_dragLineStartInstanceId.Value) is CardBoard attacker)
            {
                GetNode("/root/Main").RpcId(1, "CombatPlayerCard",
                    attacker.CardGameState.Id.ToString(),
                    cardBoard.CardGameState.Id.ToString());
            }

            _dragLineStartInstanceId = null;
        }
    }

    private void OnDragCardStart(Card card, Vector2 mousePosition)
    {
        _state = InputState.DraggingCard;
        _dragOffset = mousePosition - card.Position;
        _cardToDrag = card;
        card.ZIndex = 1;
    }

    private void OnDragCardEnd()
    {
        _cardToDrag!.ZIndex = 0;
        _cardToDrag!.PlayOrInvokeDragFailure();
        _cardToDrag = null;
        _dragOffset = Vector2.Zero;
        _state = InputState.Idle;
    }
}