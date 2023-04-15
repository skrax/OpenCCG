using Godot;

namespace OpenCCG;

public partial class EnemyAvatar : Sprite2D
{
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