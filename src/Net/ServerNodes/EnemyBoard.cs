using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG.Net.ServerNodes;

public partial class EnemyBoard : Node, IEnemyBoardRpc
{
    [Rpc]
    public void PlaceCard(string cardGameStateJson)
    {
    }

    [Rpc]
    public void UpdateCard(string cardGameStateUpdateJson)
    {
    }

    [Rpc]
    public void RemoveCard(string id)
    {
    }
}