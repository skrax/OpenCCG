using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Data;

public class ExposeCreatureCardEffect : ICardEffect
{
    public const string Id = "TEST-E-008";

    public ExposeCreatureCardEffect(string initJson)
    {
        var init = JsonSerializer.Deserialize<Init>(initJson);

        RequireTargetSide = init!.Side;
    }

    public RequireTargetSide RequireTargetSide;

    public string GetText()
    {
        return $@"Expose {
            RequireTargetSide switch
            {
                RequireTargetSide.All => "a",
                RequireTargetSide.Friendly => "a friendly",
                RequireTargetSide.Enemy => "an enemy",
                _ => throw new ArgumentOutOfRangeException()
            }} creature";
    }

    public async Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState)
    {
        var cardDto = card.AsDto();
        playerGameState.Nodes.MidPanel.EndTurnButtonSetActive(playerGameState.PeerId,
            new EndTurnButtonSetActiveDto(false, "Select a target"));
        var input = new RequireTargetInputDto(cardDto, RequireTargetType.Creature, RequireTargetSide);
        var output = await playerGameState.Nodes.CardEffectPreview.RequireTargetsAsync(playerGameState.PeerId, input);
        playerGameState.Nodes.MidPanel.EndTurnButtonSetActive(playerGameState.PeerId,
            new EndTurnButtonSetActiveDto(playerGameState.IsTurn, null));
        playerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, cardDto);

        // check if targeting was skipped
        if (output.cardId == null)
        {
            return;
        }

        var targetCard = RequireTargetSide switch
        {
            RequireTargetSide.Friendly => playerGameState.Board.Single(x => x.Id == output.cardId),
            RequireTargetSide.Enemy =>
                playerGameState.Enemy.Board.Single(x => x.Id == output.cardId),
            RequireTargetSide.All =>
                playerGameState.Board.SingleOrDefault(x => x.Id == output.cardId)
                ?? playerGameState.Enemy.Board.Single(x => x.Id == output.cardId)
        };

        await ExposeCreatureAsync(targetCard);
    }

    private static async Task ExposeCreatureAsync(CardGameState targetCard)
    {
        targetCard.IsSummoningProtectionOn = false;
        await targetCard.UpdateCreatureAsync();
    }

    public static CardEffectRecord MakeRecord(RequireTargetSide side)
    {
        return new CardEffectRecord(Id, JsonSerializer.Serialize(new Init(side)));
    }

    private record Init(RequireTargetSide Side);
}