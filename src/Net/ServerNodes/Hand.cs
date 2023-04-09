using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG.Net.ServerNodes;

public partial class Hand : Node, IHandRpc
{
    [Rpc]
    public void DrawCard(string cardGameStateJson)
    {
    }

    [Rpc]
    public void RemoveCard(string id)
    {
    }
}