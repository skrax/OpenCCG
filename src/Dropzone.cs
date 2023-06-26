using Godot;

namespace OpenCCG;

public partial class Dropzone : Control
{
    public override void _Ready()
    {
        EventSink.OnDragCardStart += () => MouseFilter = MouseFilterEnum.Stop;
        EventSink.OnDragCardStop += () => MouseFilter = MouseFilterEnum.Ignore;
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