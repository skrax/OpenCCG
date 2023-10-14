using System.IO;
using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Menu : Node
{
    private static readonly PackedScene CardBrowserScene = GD.Load<PackedScene>("res://scenes/card-browser.tscn");
    private static readonly PackedScene MainScene = GD.Load<PackedScene>("res://scenes/main.tscn");
    [Export] private Label _deckNameLabel = null!;
    [Export] private FileDialog _fileDialog = null!;
    [Export] private LineEdit _password = null!;
    [Export] private Button _playButton = null!, _cardsButton = null!, _loadDeckButton = null!;
    private RuntimeData _runtimeData = null!;
    [Export] private CheckButton _usePasswordCheckButton = null!;

    public override void _Ready()
    {
        _runtimeData = this.GetAutoloaded<RuntimeData>();
        _password.Text = _runtimeData.QueuePassword ?? string.Empty;
        _usePasswordCheckButton.ButtonPressed = _runtimeData.UseQueuePassword;
        if (_runtimeData.DeckPath != null)
        {
            SetDeck(_runtimeData.DeckPath);
        }
        else
        {
            _deckNameLabel.Text = "No Deck Selected";
            _deckNameLabel.AddThemeColorOverride("font_color", Colors.Red);
        }

        _playButton.Pressed += () =>
        {
            if (_runtimeData.DeckPath == null) return;
            if (_usePasswordCheckButton.ButtonPressed && _password.Text == string.Empty) return;

            GetTree().ChangeSceneToPacked(MainScene);
        };

        _cardsButton.Pressed += () => { GetTree().ChangeSceneToPacked(CardBrowserScene); };
        _usePasswordCheckButton.Pressed += () => _runtimeData.UseQueuePassword = _usePasswordCheckButton.ButtonPressed;
        _password.TextChanged += s => _runtimeData.QueuePassword = s;

        _loadDeckButton.Pressed += () => _fileDialog.Visible = true;
        _fileDialog.FileSelected += SetDeck;
    }

    private void SetDeck(string path)
    {
        _runtimeData.DeckPath = path;
        _deckNameLabel.Text = Path.GetFileNameWithoutExtension(path);
        _deckNameLabel.AddThemeColorOverride("font_color", Colors.Green);
        _playButton.Disabled = false;
    }
}