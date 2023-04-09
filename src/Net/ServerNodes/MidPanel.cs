using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG.Net.ServerNodes;

public partial class MidPanel : Node, IMidPanelRpc
{
    [Rpc]
    public void EndTurnButtonSetActive(bool isActive)
    {
        throw new System.NotImplementedException();
    }
}