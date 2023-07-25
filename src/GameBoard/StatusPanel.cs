using System;
using System.Threading.Tasks;
using Godot;

namespace OpenCCG.GameBoard;

public partial class StatusPanel : Node
{
    [Export] public Avatar _avatar;
    [Export] private Label _cardCountLabel;
    [Export] private Panel _dmgPopup;
    [Export] private Label _energyLabel;
    [Export] private Label _healthLabel;
    [Export] private bool _isEnemy;

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