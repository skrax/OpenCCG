using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Matchmaking;
using Serilog;

namespace OpenCCG.Net.Gameplay;

[GlobalClass]
public partial class SessionManager : Node
{
    [Export] private TestServer _server = null!;

    public override void _Ready()
    {
        Log.Information(Logging.Templates.ServiceIsRunning, nameof(SessionManager));
    }

    public async void CreateSession(QueuedPlayer player1, QueuedPlayer player2)
    {
        var session = new Session(player1, player2, _server);
        AddChild(session);
        session.Configure();
        session.AddToGroup($"Session-{player1.PeerId}");
        session.AddToGroup($"Session-{player2.PeerId}");
        session.AddToGroup($"Session-{session.Context.SessionId}");

        var canBegin = await session.TryPreparePlayers();
        if (!canBegin)
        {
            if (!session.IsQueuedForDeletion()) session.QueueFree();
            return;
        }

        session.Begin();
    }

    public void DissolveSession(long peerId)
    {
        var session = GetTree().GetFirstNodeInGroup($"Session-{peerId}");
        if (session is null || session.IsQueuedForDeletion()) return;

        session.QueueFree();
    }
}