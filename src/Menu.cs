using Godot;

namespace OpenCCG;

public partial class Menu : Node
{
    private static readonly PackedScene CardBrowserScene = GD.Load<PackedScene>("res://scenes/card-browser.tscn");
    private static readonly PackedScene MainScene = GD.Load<PackedScene>("res://scenes/main.tscn");
    private Button _playButton, _cardsButton;

    public override void _Ready()
    {
        _playButton = GetChild<Button>(0);
        _cardsButton = GetChild<Button>(1);

        _playButton.Pressed += () => { GetTree().ChangeSceneToPacked(MainScene); };

        _cardsButton.Pressed += () => { GetTree().ChangeSceneToPacked(CardBrowserScene); };
    }
}