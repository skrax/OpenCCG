using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net;

public partial class StatusPanel : Node, IMessageReceiver<MessageType>
{
    private Label _healthLabel;
    private Label _cardCountLabel;
    private Label _energyLabel;

    public override void _Ready()
    {
        _healthLabel = GetChild<Label>(0);
        _cardCountLabel = GetChild<Label>(2);
        _energyLabel = GetChild<Label>(4);
    }

    private void SetEnergy(int value)
    {
        _energyLabel.Text = value.ToString();
    }

    private void SetCardCount(int value)
    {
        _cardCountLabel.Text = value.ToString();
    }

    private void SetHealth(int value)
    {
        _healthLabel.Text = value.ToString();
    }

    public Dictionary<string, IObserver>? Observers => null;
    
    [Rpc]
    public void HandleMessage(string messageJson)
    {
        IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
    }

    public Func<int, string?, string?> GetExecutor(MessageType messageType) => messageType switch
    {
        MessageType.SetEnergy => IMessageReceiver<MessageType>.MakeExecutor<int>(SetEnergy),
        MessageType.SetCardCount => IMessageReceiver<MessageType>.MakeExecutor<int>(SetCardCount),
        MessageType.SetHealth => IMessageReceiver<MessageType>.MakeExecutor<int>(SetHealth),
    };
}