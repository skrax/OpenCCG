using Godot;
using OpenCCG.Net.Api;

public partial class StatusPanel : Node, IStatusPanelRpc
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

    [Rpc]
    public void SetEnergy(int value)
    {
        _energyLabel.Text = value.ToString();
    }

    [Rpc]
    public void SetCardCount(int value)
    {
        _cardCountLabel.Text = value.ToString();
    }

    [Rpc]
    public void SetHealth(int value)
    {
        _healthLabel.Text = value.ToString();
    }
}