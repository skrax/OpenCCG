using Godot;

namespace OpenCCG;

public partial class Dropzone : Control
{
    public override void _Ready()
    {
        EventSink.OnDragCardStart += StopMouse;
        EventSink.OnDragCardStop += IgnoreMouse;
    }

    public override void _ExitTree()
    {
        EventSink.OnDragCardStart -= StopMouse;
        EventSink.OnDragCardStop -= IgnoreMouse;
    }

    private void IgnoreMouse()
    {
        MouseFilter = MouseFilterEnum.Ignore;
    }

    private void StopMouse()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }


    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var obj = InstanceFromId(data.As<ulong>());
        return obj is Card;
    }

    public override async void _DropData(Vector2 atPosition, Variant data)
    {
        var card = InstanceFromId(data.As<ulong>()) as Card;
        await card!.PlayAsync();
    }
}