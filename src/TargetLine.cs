using Godot;

namespace OpenCCG;

public partial class TargetLine : Line2D
{
    [Export] private Sprite2D _arrow;
    private Control? _dst;

    public void Target(Control src, Control dst)
    {
        _dst = dst;
        Visible = true;
        SetProcess(true);
        SetPointPosition(0, src.GetGlobalRect().GetCenter());
        var dstPos = dst.GetGlobalRect().GetCenter();
        SetPointPosition(1, dstPos);
        _arrow.Position = dstPos;
    }

    public void Reset()
    {
        _dst = null;
        Visible = false;
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        var dstPos = _dst!.GetGlobalRect().GetCenter();
        SetPointPosition(1, dstPos);
        _arrow.Position = dstPos;
    }

    public override void _Ready()
    {
        SetProcess(false);
    }
}