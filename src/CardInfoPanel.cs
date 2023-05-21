using Godot;

namespace OpenCCG;

public partial class CardInfoPanel : Panel
{
    [Export] private Label _descriptionLabel;

    public string Value
    {
        get => _descriptionLabel.Text;
        set => _descriptionLabel.Text = value;
    }
}