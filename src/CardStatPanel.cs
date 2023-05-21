using Godot;

namespace OpenCCG;

public partial class CardStatPanel : Panel
{
    [Export] private Label _statLabel;

    public int Value
    {
        get => int.Parse(_statLabel.Text);
        set => _statLabel.Text = $"{value}";
    }
}