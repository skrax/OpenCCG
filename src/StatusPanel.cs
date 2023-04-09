using Godot;
using OpenCCG.Net.Api;

public partial class StatusPanel : Node, IStatusPanelRpc
{
    private Label _cardCountLabel;
    private Label _energyLabel;

    public override void _Ready()
    {
        _cardCountLabel = GetChild<Label>(0);
        _energyLabel = GetChild<Label>(2);
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
}