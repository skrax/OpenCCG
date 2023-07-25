using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Gameplay.Dto;
using OpenCCG.Net.Messaging;
using Serilog;

namespace OpenCCG.Net.Gameplay;

[GlobalClass]
public partial class SessionClient : Node
{
    [Export] private MessageBroker _broker = null!;

    public override void _Ready()
    {
        Configure();
    }

    public void Configure()
    {
        _broker.MapAwaitableResponse(Route.PlayCardResponse);
        _broker.MapAwaitableResponse(Route.CombatPlayerResponse);
        _broker.MapAwaitableResponse(Route.CombatPlayerCardResponse);
        _broker.MapAwaitableResponse(Route.EndTurnResponse);
        _broker.Map(Route.MatchFound, OnMatchFound);
        _broker.Map(Route.EnableEndTurnButton, OnEnableEndTurnButton);
        _broker.Map(Route.AddCardToHand, OnAddCardToHand);
        _broker.Map(Route.AddCardToBoard, OnAddCardToBoard);
        _broker.Map(Route.RemoveCardFromHand, OnRemoveCardFromHand);
        _broker.Map(Route.RemoveCardFromBoard, OnRemoveCardFromBoard);
        _broker.Map(Route.SetEnergy, OnSetEnergy);
        _broker.Map(Route.SetHealth, OnSetHealth);
        _broker.Map(Route.SetMaxEnergy, OnSetMaxEnergy);
        _broker.Map(Route.SetCardsInHand, OnSetCardsInHand);
        _broker.Map(Route.SetCardsInDeck, OnSetCardsInDeck);
        Log.Information(Logging.Templates.ServiceIsRunning, nameof(SessionClient));
    }

    private MessageControllerResult? OnSetCardsInDeck(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnSetCardsInHand(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnSetHealth(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnSetMaxEnergy(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnSetEnergy(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnRemoveCardFromBoard(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnRemoveCardFromHand(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnAddCardToBoard(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult? OnAddCardToHand(MessageContext context)
    {
        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnEnableEndTurnButton(MessageContext context)
    {
        Log.Information("End turn button enabled");

        return MessageControllerResult.AsResult();
    }

    private MessageControllerResult OnMatchFound(MessageContext context)
    {
        if (context.Message.TryUnwrap(out MatchInfoDto matchInfoDto))
        {
            Log.Information("Match found: {SessionId} {PlayerId} {EnemyPlayerId}",
                matchInfoDto.SessionId, matchInfoDto.PlayerId, matchInfoDto.EnemyPlayerId);
        }

        return MessageControllerResult.AsResult();
    }
}