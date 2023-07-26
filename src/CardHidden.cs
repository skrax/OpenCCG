using System;
using System.Threading.Tasks;
using Godot;

namespace OpenCCG;

public partial class CardHidden : TextureRect
{
    [Export] private Curve _drawCurve;

    public override void _Ready()
    {
        Modulate = Colors.Transparent;
    }

    public async Task PlayDrawAnimAsync(Vector2 start, Vector2 end)
    {
        Modulate = Colors.White;
        var t = 0f;
        const float delta = 1000 / 60f;
        while (t < 1f)
        {
            t += delta / 360;
            t = Math.Clamp(t, 0f, 1f);

            var pos = GlobalPosition;
            pos.X = Mathf.Lerp(start.X, end.X, t);
            var lol = (pos.X - end.X) / (start.X - end.X);

            var h = _drawCurve.Sample(lol) * 200f;
            pos.Y = start.Y + h;

            GlobalPosition = pos;
            await Task.Delay(TimeSpan.FromMilliseconds(delta));
        }
    }
}