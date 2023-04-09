using Godot;

namespace OpenCCG;

public partial class CardInfoPanel : Panel
{
    private Label _descriptionLabel;

    public string Description
    {
        get => _descriptionLabel.Text;
        set => _descriptionLabel.Text = value;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _descriptionLabel = GetChild<Label>(0);
    }
}