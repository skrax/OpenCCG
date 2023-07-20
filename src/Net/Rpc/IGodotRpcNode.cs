using Godot;

namespace OpenCCG.Net.Rpc;

public interface IGodotRpcNode
{
    MultiplayerApi Multiplayer { get; }

    Godot.Error RpcId(long peerId, StringName method, params Variant[] args);
}