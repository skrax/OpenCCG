using Godot;

namespace OpenCCG;

public partial class DeckCountProgressBar : ProgressBar
{
    [Export] private Label _countLabel;

    public override void _ValueChanged(double newValue)
    {
        var val = (int)newValue;
        _countLabel.Visible = val >= 0;
        _countLabel.Text = val > 8 ? "9+" : $"{val}";
    }

    public int Count
    {
        get => (int)Value;
        set => Value = value;
    }
}