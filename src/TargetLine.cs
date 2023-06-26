using Godot;

namespace OpenCCG;

public partial class TargetLine : Line2D
{
    [Export] private Sprite2D _arrow;
    private Control? _dst;

    public void Target(Control src, Control dst)
    {
        _dst = dst;
        SetPointPosition(0, src.GetGlobalRect().GetCenter());
        var dstPos = dst.GetGlobalRect().GetCenter();
        SetPointPosition(1, dstPos);
        _arrow.Position = dstPos;
        SetProcess(true);
        Visible = true;
    }

    public void Reset()
    {
        SetProcess(false);
        _dst = null;
        Visible = false;
        SetPointPosition(0, Vector2.Zero);
        SetPointPosition(1, Vector2.Zero);
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