using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class CardHidden : TextureRect
{
    public bool _drawAnim;
    [Export] private Curve _drawCurve;

    public async void PlayDrawAnimAsync(Vector2 start, Vector2 end, TaskCompletionSource taskCompletionSource)
    {
        if (!_drawAnim) return;
        _drawAnim = false;
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

        taskCompletionSource.SetResult();
    }
}