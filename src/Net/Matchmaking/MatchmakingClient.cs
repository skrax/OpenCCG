using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Matchmaking;

[GlobalClass]
public partial class MatchmakingClient : Node
{
    [Export] private MessageBroker _broker = null!;
    [Export] private GameBoard.MidPanel _midPanel = null!;

    public override void _Ready()
    {
        Configure();
    }

    public void Configure()
    {
        _broker.MapAwaitableResponse(Route.EnqueueResponse);
        Log.Information(Logging.Templates.ServiceIsRunning, nameof(MatchmakingClient));
    }

    public async void EnqueuePlayer(SavedDeck deck, string? password = null)
    {
        var dto = new MatchmakingRequest(deck, password);
        var message = Message.CreateWithResponse(Route.Enqueue, Route.EnqueueResponse, dto);
        var response = await _broker.EnqueueMessageAndGetResponseAsync(1, message);

        if (response != null)
        {
            if (response.Message.HasError())
            {
                Log.Error("Failed to enqueue");
                _midPanel.SetStatusMessage("Connection Failed");
            }
            else
            {
                Log.Information("Enqueued");
                _midPanel.SetStatusMessage("Looking for Opponent ..");
            }
        }
        else
        {
            Log.Error("Failed to enqueue");
            _midPanel.SetStatusMessage("Connection Failed");
        }
    }
}