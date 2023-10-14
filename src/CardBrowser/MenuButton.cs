using Godot;

namespace OpenCCG.CardBrowser;

public partial class MenuButton : Button
{
    [Export] private PackedScene _menuScene = null!;
    public override void _Ready()
    {
        Pressed += () => GetTree().ChangeSceneToPacked(_menuScene);
    }
}