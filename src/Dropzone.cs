using Godot;

namespace OpenCCG;

public partial class Dropzone : Control
{
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var card = InstanceFromId(data.As<ulong>()) as Card;
        card!.Play();
    }
}