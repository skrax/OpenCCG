using Godot;

namespace OpenCCG;

public partial class SkipSelectionField : Button
{
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return Visible &&
               InstanceFromId(data.As<ulong>()) is CardEffectPreview;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        Disable();
        var preview = InstanceFromId(data.As<ulong>()) as CardEffectPreview;
        preview!.SkipTarget();
    }

    public void Enable()
    {
        Visible = true;
    }

    private void Disable()
    {
        Visible = false;
    }
}