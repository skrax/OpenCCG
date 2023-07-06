using Godot;

namespace OpenCCG;

public partial class CardInfoPanel : Panel
{
    [Export] private RichTextLabel _descriptionLabel;

    public string Value
    {
        get => _descriptionLabel.Text;
        set => _descriptionLabel.Text = value;
    }
}