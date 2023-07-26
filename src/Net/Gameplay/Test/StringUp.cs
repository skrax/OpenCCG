using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net.Gameplay.Test;

public class StringUp : Spell
{
    public StringUp(SpellOutline outline) : base(outline)
    {
    }

    public override void OnPlay()
    {
        throw new System.NotImplementedException();
    }
    #if false

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
                is not Creature card) goto RETRY;

            PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());

            card.MoveToZone(CardZone.None);
            await card.RemoveFromBoardAsync();
            await card.OnExitAsync();
            card.MoveToZone(CardZone.Pit);
        }
    }
#endif
}