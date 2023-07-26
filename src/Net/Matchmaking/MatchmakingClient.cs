using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Gameplay;
using OpenCCG.Net.Gameplay.Dto;
using OpenCCG.Net.Messaging;
using Serilog;
using Error = OpenCCG.Net.Messaging.Error;

namespace OpenCCG.Net.Matchmaking;

[GlobalClass]
public partial class MatchmakingClient : Node
{
    [Export] private MessageBroker _broker = null!;
    [Export] private GameBoard.MidPanel _midPanel = null!;
    [Export] private GameBoard.StatusPanel _statusPanel = null!;
    [Export] private GameBoard.StatusPanel _enemyStatusPanel = null!;
    [Export] private GameBoard.Hand _hand = null!;
    [Export] private GameBoard.EnemyHand _enemyHand = null!;

    public override void _Ready()
    {
        Configure();
    }

    public void Configure()
    {
        _broker.MapAwaitableResponse(Route.EnqueueResponse);
        _broker.Map(Route.MatchFound, OnMatchFound);
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
                _midPanel?.SetStatusMessage("Connection Failed");
            }
            else
            {
                Log.Information("Enqueued");
                _midPanel?.SetStatusMessage("Looking for Opponent ..");
            }
        }
        else
        {
            Log.Error("Failed to enqueue");
            _midPanel?.SetStatusMessage("Connection Failed");
        }
    }

    private MessageControllerResult OnMatchFound(MessageContext context)
    {
        if (!context.Message.TryUnwrap(out MatchInfoDto matchInfoDto))
        {
            Log.Error("Failed to unwrap message for {Route}", Route.MatchFound);
            return MessageControllerResult.AsError(Error.FromCode(ErrorCode.InternalServerError));
        }

        Log.Information("Match found: {SessionId} {PlayerId} {EnemyPlayerId}",
            matchInfoDto.SessionId, matchInfoDto.PlayerId, matchInfoDto.EnemyPlayerId);

        _midPanel.SetStatusMessage("Match Found");
        var sessionClient = new SessionClient(_broker, matchInfoDto,
            _midPanel, _statusPanel, _enemyStatusPanel,
            _hand, _enemyHand);
        AddChild(sessionClient);

        return MessageControllerResult.AsResult();
    }
}