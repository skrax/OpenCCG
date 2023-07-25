using Godot;

namespace OpenCCG.GameBoard;

public partial class MidPanel : Control
{
    [Export] private Button _endTurnButton, _exitButton;
    [Export] private PackedScene _menuScene;
    [Export] private Label _statusLabel;

    public override void _Ready()
    {
        _endTurnButton.Pressed += OnEndTurnPressed;
        _exitButton.Pressed += () => GetTree().ChangeSceneToPacked(_menuScene);
    }

    private void OnEndTurnPressed()
    {
        //GetNode<Main>("/root/Main").EndTurn();
    }

    public void SetStatusMessage(string message)
    {
        _statusLabel.Text = message;
    }

    public void EndTurnButtonSetActive(EndTurnButtonSetActiveDto dto)
    {
        _endTurnButton.Disabled = !dto.isActive;
        if (dto.reason != null)
            _endTurnButton.Text = dto.reason;
        else
            _endTurnButton.Text = dto.isActive ? "End Turn" : "Enemy Turn";
    }
}