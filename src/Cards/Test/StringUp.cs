using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Cards.Test;

public class StringUp : SpellImplementation
{
    public StringUp(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        var input = new RequireTargetInputDto(AsDto(), RequireTargetType.Creature, RequireTargetSide.All);
        RETRY:
        var output = await PlayerGameState.Nodes.CardEffectPreview.RequireTargetsAsync(PlayerGameState.PeerId, input);

        if (output.isEnemyAvatar.HasValue) goto RETRY;
        if (output.cardId.HasValue)
        {
            if ((PlayerGameState.Board.SingleOrDefault(x => x.Id == output.cardId) ??
                 PlayerGameState.Enemy.Board.SingleOrDefault(x => x.Id == output.cardId))
                is not CreatureImplementation card) goto RETRY;

            PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());

            card.MoveToZone(CardZone.None);
            await card.RemoveFromBoardAsync();
            await card.OnExitAsync();
            card.MoveToZone(CardZone.Pit);
        }
    }
}