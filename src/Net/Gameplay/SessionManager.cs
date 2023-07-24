using Godot;
using OpenCCG.Net.Matchmaking;
using Serilog;

namespace OpenCCG.Net.Gameplay;

[GlobalClass]
public partial class SessionManager : Node
{
    [Export] private TestServer _server = null!;

    public override void _Ready()
    {
        Log.Information("{Service} is running", nameof(SessionManager));
    }

    public void CreateSession(QueuedPlayer player1, QueuedPlayer player2)
    {
        var session = new Session(player1, player2);
        AddChild(session);
        session.Configure(_server);
        
        session.AddToGroup($"Session-{player1.PeerId}");
        session.AddToGroup($"Session-{player2.PeerId}");
        session.AddToGroup($"Session-{session.Context.SessionId}");
    }

    public void DissolveSession(long peerId)
    {
        GetTree().GetFirstNodeInGroup($"Session-{peerId}")?.QueueFree();
    }
}