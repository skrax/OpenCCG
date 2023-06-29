using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public class HeedTheCallCardEffect : ICardEffect
{
    public const string Id = "TEST-E-007";

    public string GetText()
    {
        return "Summon 2 Mornehold Spectres";
    }

    public async Task ExecuteAsync(CardGameState _, PlayerGameState playerGameState)
    {
        var record = Database.Cards["TEST-012"];

        for (var i = 0; i < 2; ++i)
        {
            var spectre = new CardGameState(record, playerGameState);

            spectre.IsSummoningProtectionOn = spectre.Record.Abilities is { Exposed: false, Defender: false };
            spectre.IsSummoningSicknessOn = !spectre.Record.Abilities.Haste;

            await spectre.OnEnterAsync(playerGameState);

            spectre.Zone = CardZone.Board;
            playerGameState.Board.AddLast(spectre);

            var dto = spectre.AsDto();

            playerGameState.Nodes.Board.PlaceCard(playerGameState.PeerId, dto);
            playerGameState.Nodes.Hand.RemoveCard(playerGameState.PeerId, spectre.Id);

            playerGameState.Nodes.EnemyBoard.PlaceCard(playerGameState.EnemyPeerId, dto);
            playerGameState.Nodes.EnemyHand.RemoveCard(playerGameState.EnemyPeerId);
        }

        playerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(playerGameState.EnemyPeerId, _.AsDto());
    }
}