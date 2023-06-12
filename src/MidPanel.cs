using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net;

namespace OpenCCG;

public partial class MidPanel : Node, IMessageReceiver<MessageType>
{
    private Button _endTurnButton;

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

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessage(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType) => messageType switch
    {
        MessageType.EndTurnButtonSetActive => IMessageReceiver<MessageType>.MakeExecutor<bool>(EndTurnButtonSetActive)
    };
}