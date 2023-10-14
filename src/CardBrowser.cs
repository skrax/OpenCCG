using Godot;

namespace OpenCCG;

public partial class CardBrowser : Control
{
    private static readonly PackedScene MenuScene = GD.Load<PackedScene>("res://scenes/menu.tscn");

    [Export] private ScrollContainer  _deckScrollContainer;

    [Export] private FileDialog _fileDialog;
    [Export] private Button _menuButton, _createDeckButton, _saveDeckButton, _loadDeckButton;

    public override void _Ready()
    {
//        _menuButton.Pressed += () => { GetTree().ChangeSceneToPacked(MenuScene); };
//
//        _fileDialog.FileSelected += LoadDeck;
//        _loadDeckButton.Pressed += () => { _fileDialog.Visible = true; };
//
//        _createDeckButton.Pressed += ClearDeck;
//
//        _saveDeckButton.Pressed += SaveDeck;
//
//        ResetCounters();
    }


}