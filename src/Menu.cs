using System.IO;
using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Menu : Node
{
    private static readonly PackedScene CardBrowserScene = GD.Load<PackedScene>("res://scenes/card-browser.tscn");
    private static readonly PackedScene MainScene = GD.Load<PackedScene>("res://scenes/main.tscn");
    [Export] private Label _deckNameLabel;
    [Export] private FileDialog _fileDialog;
    [Export] private LineEdit _password;
    [Export] private Button _playButton, _cardsButton, _loadDeckButton;
    private RuntimeData _runtimeData;
    [Export] private CheckButton _usePasswordCheckButton;

    public override void _Ready()
    {
        _runtimeData = this.GetAutoloaded<RuntimeData>();
        _password.Text = _runtimeData._queuePassword ?? string.Empty;
        _usePasswordCheckButton.ButtonPressed = _runtimeData._useQueuePassword;
        if (_runtimeData._deckPath != null)
        {
            SetDeck(_runtimeData._deckPath);
        }
        else
        {
            _deckNameLabel.Text = "No Deck Selected";
            _deckNameLabel.AddThemeColorOverride("font_color", Colors.Red);
        }

        _playButton.Pressed += () =>
        {
            if (_runtimeData._deckPath == null) return;
            if (_usePasswordCheckButton.ButtonPressed && _password.Text == string.Empty) return;

            GetTree().ChangeSceneToPacked(MainScene);
        };

        _cardsButton.Pressed += () => { GetTree().ChangeSceneToPacked(CardBrowserScene); };
        _usePasswordCheckButton.Pressed += () => _runtimeData._useQueuePassword = _usePasswordCheckButton.ButtonPressed;
        _password.TextChanged += s => _runtimeData._queuePassword = s;

        _loadDeckButton.Pressed += () => _fileDialog.Visible = true;
        _fileDialog.FileSelected += SetDeck;
    }

    private void SetDeck(string path)
    {
        _runtimeData._deckPath = path;
        _deckNameLabel.Text = Path.GetFileNameWithoutExtension(path);
        _deckNameLabel.AddThemeColorOverride("font_color", Colors.Green);
        _playButton.Disabled = false;
    }
}