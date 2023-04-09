using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG;

public partial class MidPanel : Node, IMidPanelRpc
{
    private Button _endTurnButton;

    public override void _Ready()
    {
        _endTurnButton = GetChild<Button>(0);
        _endTurnButton.Pressed += OnEndTurnPressed;
    }

    private void OnEndTurnPressed()
    {
        GetNode("/root/Main").RpcId(1, "EndTurn");
    }

    [Rpc]
    public void EndTurnButtonSetActive(bool isActive)
    {
        _endTurnButton.Disabled = !isActive;
        _endTurnButton.Text = isActive ? "End Turn" : "Enemy Turn";
    }
}