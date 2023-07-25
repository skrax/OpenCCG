using OpenCCG.Core;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Gameplay;

public class SessionClient : MessageClient
{
    public SessionClient(IMessageBroker broker) : base(broker)
    {
    }

    public override void Configure()
    {
        Broker.MapAwaitableResponse(Route.PlayCardResponse);
        Broker.MapAwaitableResponse(Route.CombatPlayerResponse);
        Broker.MapAwaitableResponse(Route.CombatPlayerCardResponse);
        Broker.MapAwaitableResponse(Route.EndTurnResponse);
        Broker.Map(Route.MatchFound, OnMatchFound);
        Broker.Map(Route.EnableEndTurnButton, OnEnableEndTurnButton);
    }

    private MessageControllerResult OnEnableEndTurnButton(MessageContext context)
    {
        Log.Information("End turn button enabled");
        
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnMatchFound(MessageContext context)
    {
        if (context.Message.TryUnwrap(out SessionContext sessionContext))
        {
            Log.Information("Match found: {SessionId}", sessionContext.SessionId);
        }

        return MessageControllerResult.AsResult();
    }
}