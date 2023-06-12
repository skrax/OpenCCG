using System.Collections.Generic;
using Godot;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG;

public partial class MidPanel : Node, IMessageReceiver<MessageType>
{
    private Button _endTurnButton;

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.EndTurnButtonSetActive => Executor.Make<bool>(EndTurnButtonSetActive)
        };
    }

    public override void _Ready()
    {
        _endTurnButton = GetChild<Button>(0);
        _endTurnButton.Pressed += OnEndTurnPressed;
    }

    private void OnEndTurnPressed()
    {
        GetNode<Main>("/root/Main").EndTurn();
    }

    public void EndTurnButtonSetActive(bool isActive)
    {
        _endTurnButton.Disabled = !isActive;
        _endTurnButton.Text = isActive ? "End Turn" : "Enemy Turn";
    }
}