using Godot;

namespace OpenCCG;

public partial class CardStatPanel : Panel
{
    [Export] private Label _statLabel;

    public int Value
    {
        get => int.Parse(_statLabel.Text);
        private set => _statLabel.Text = $"{value}";
    }

    public void SetValue(int currentValue, int originalValue)
    {
        Value = currentValue;
        if (currentValue < originalValue)
        {
            _statLabel.Modulate = Colors.Red;
        }
        else if (currentValue > originalValue)
        {
            _statLabel.Modulate = Colors.Green;
        }
        else
        {
            _statLabel.Modulate = Colors.White;
        }
    }
}