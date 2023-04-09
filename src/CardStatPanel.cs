using Godot;

namespace OpenCCG;

public partial class CardStatPanel : Panel
{
    private Label _statLabel;

    public int Value
    {
        get => int.Parse(_statLabel.Text);
        set => _statLabel.Text = $"{value}";
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _statLabel = GetChild<Label>(0);
    }
}