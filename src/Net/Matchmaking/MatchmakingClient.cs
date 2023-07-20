using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Matchmaking;

public class MatchmakingClient : MessageClient
{
    public MatchmakingClient(IMessageBroker broker) : base(broker)
    {
    }

    public override void Configure()
    {
        Broker.MapAwaitableResponse(Route.EnqueueResponse);
    }

    public async void EnqueuePlayer(SavedDeck deck, string? password = null)
    {
        var dto = new MatchmakingRequest(deck, password);
        var message = Message.CreateWithResponse(Route.Enqueue, Route.EnqueueResponse, dto);
        var response = await Broker.EnqueueMessageAndGetResponseAsync(1, message);

        if (response != null)
        {
            if (response.Message.HasError())
                Log.Error("Failed to enqueue");
            else
                Log.Information("Enqueued");
        }
        else
        {
            Log.Error("Failed to enqueue");
        }
    }
}