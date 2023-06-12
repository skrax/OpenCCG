using System.Collections.Generic;
using Godot;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG;

public partial class StatusPanel : Node, IMessageReceiver<MessageType>
{
    private Label _cardCountLabel;
    private Label _energyLabel;
    private Label _healthLabel;

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
            MessageType.SetEnergy => Executor.Make<int>(SetEnergy),
            MessageType.SetCardCount => Executor.Make<int>(SetCardCount),
            MessageType.SetHealth => Executor.Make<int>(SetHealth)
        };
    }

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
}