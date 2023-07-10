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
    [Export] public Avatar _avatar;
    [Export] private Label _cardCountLabel;
    [Export] private Panel _dmgPopup;
    [Export] private Label _energyLabel;
    [Export] private Label _healthLabel;
    [Export] private bool _isEnemy;

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
            MessageType.SetEnergy => Executor.Make<SetEnergyDto>(SetEnergy, Executor.ResponseMode.NoResponse),
            MessageType.SetCardCount => Executor.Make<int>(SetCardCount, Executor.ResponseMode.NoResponse),
            MessageType.SetHealth => Executor.Make<int>(SetHealth, Executor.ResponseMode.NoResponse),
            _ => null
        };
    }

    public override void _Ready()
    {
        _avatar.IsEnemy = _isEnemy;
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