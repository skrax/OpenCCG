using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG.Net.ServerNodes;

public partial class StatusPanel : Node, IStatusPanelRpc
{
    [Rpc]
    public void SetEnergy(int value)
    {
    }

    [Rpc]
    public void SetCardCount(int value)
    {
    }
}