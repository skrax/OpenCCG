using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG.Net.ServerNodes;

public partial class Board : Node, IBoardRpc
{
    [Rpc]
    public void PlaceCard(string cardGameStateJson)
    {
    }
}