using System;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Matchmaking;

public class MatchmakingClient : IMessageController
{
    public void Configure(IMessageBroker broker)
    {
        broker.MapResponseForTask(Route.EnqueueResponse);
    }

    public async void EnqueuePlayer(IMessageBroker broker)
    {
        var deck = new SavedDeck(string.Empty, Array.Empty<CardUIDeck.JsonRecord>());
        var dto = new MatchmakingRequest(deck, "secret");
        var response = await broker.EnqueueMessageAndGetResponseAsync(1,
            Message.CreateWithResponse(Route.Enqueue, Route.EnqueueResponse, dto));
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