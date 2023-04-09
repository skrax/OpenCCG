using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG.Net.ServerNodes;

public partial class EnemyHand : Node, IEnemyHandRpc
{
    [Rpc]
    public void RemoveCard()
    {
    }

    [Rpc]
    public void DrawCard()
    {
    }
}