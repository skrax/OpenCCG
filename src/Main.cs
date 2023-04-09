using Godot;
using OpenCCG.Net.Api;

namespace OpenCCG;

public partial class Main : Node, IMainRpc
{
    [Rpc]
    public void PlayCard(string id)
    {
    }

    [Rpc]
    public void CombatPlayerCard(string attackerId, string targetId)
    {
    }
}