using System.Collections.Generic;
using Godot;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG;

public partial class MidPanel : Control, IMessageReceiver<MessageType>
{
    [Export] private Button _endTurnButton, _exitButton;
    [Export] private PackedScene _menuScene;
    [Export] private Label _statusLabel;

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor? GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.EndTurnButtonSetActive => Executor.Make<EndTurnButtonSetActiveDto>(EndTurnButtonSetActive,
                Executor.ResponseMode.NoResponse),
            MessageType.SetStatusMessage => Executor.Make<string>(SetStatusMessage, Executor.ResponseMode.NoResponse),
            _ => null
        };
    }

    private void SetStatusMessage(string message)
    {
        _statusLabel.Text = message;
    }

    public override void _Ready()
    {
        _endTurnButton.Pressed += OnEndTurnPressed;
        _exitButton.Pressed += () => GetTree().ChangeSceneToPacked(_menuScene);
    }

    private void OnEndTurnPressed()
    {
        GetNode<Main>("/root/Main").EndTurn();
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