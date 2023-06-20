using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG;

public record SetEnergyDto(int Current, int Max);

public partial class StatusPanel : Node, IMessageReceiver<MessageType>
{
    private Label _cardCountLabel;
    private Label _energyLabel;
    private Label _healthLabel;
    [Export] private Panel _dmgPopup;

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
            MessageType.SetEnergy => Executor.Make<SetEnergyDto>(SetEnergy),
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

    private void SetEnergy(SetEnergyDto dto)
    {
        _energyLabel.Text = $"{dto.Current} / {dto.Max}";
    }

    private void SetCardCount(int value)
    {
        _cardCountLabel.Text = value.ToString();
    }

    private async Task SetHealth(int value)
    {
        var diff = value - int.Parse(_healthLabel.Text);
        _healthLabel.Text = value.ToString();

        if (diff != 0)
        {
            _dmgPopup.Visible = true;
            _dmgPopup.GetChild<Label>(0).Text = diff > 0 ? $"+ {diff}" : diff.ToString();
            await Task.Delay(TimeSpan.FromSeconds(2));
            _dmgPopup.Visible = false;
        }
    }
}