using System.IO;
using Godot;

namespace OpenCCG;

public partial class Menu : Node
{
    private static readonly PackedScene CardBrowserScene = GD.Load<PackedScene>("res://scenes/card-browser.tscn");
    private static readonly PackedScene MainScene = GD.Load<PackedScene>("res://scenes/main.tscn");
    [Export] private Button _playButton, _cardsButton, _loadDeckButton;
    [Export] private Label _deckNameLabel;
    [Export] private FileDialog _fileDialog;
    [Export] private CheckButton _usePasswordCheckButton;
    [Export] private LineEdit _password;
    private string? _deckPath;
    private bool _inQueue;

    public override void _Ready()
    {
        _playButton.Pressed += () =>
        {
            if (_inQueue)
            {
                _inQueue = false;
                _playButton.Text = "Play";

                _loadDeckButton.Disabled = false;
                _cardsButton.Disabled = false;
                _usePasswordCheckButton.Disabled = false;
                _password.Editable = true;

                return;
            }

            if (_deckPath == null) return;
            if (_usePasswordCheckButton.ButtonPressed && _password.Text == string.Empty) return;

            _inQueue = true;
            _playButton.Text = "Cancel Queue";
            _loadDeckButton.Disabled = true;
            _cardsButton.Disabled = true;
            _usePasswordCheckButton.Disabled = true;
            _password.Editable = false;

            //GetTree().ChangeSceneToPacked(MainScene);
        };

        _cardsButton.Pressed += () => { GetTree().ChangeSceneToPacked(CardBrowserScene); };

        _deckNameLabel.Text = "No Deck Selected";
        _deckNameLabel.AddThemeColorOverride("font_color", Colors.Red);

        _loadDeckButton.Pressed += () => _fileDialog.Visible = true;
        _fileDialog.FileSelected += SetDeck;
    }

    private void SetDeck(string path)
    {
        _deckPath = path;
        _deckNameLabel.Text = Path.GetFileNameWithoutExtension(path);
        _deckNameLabel.AddThemeColorOverride("font_color", Colors.Green);
        _playButton.Disabled = false;
    }
}