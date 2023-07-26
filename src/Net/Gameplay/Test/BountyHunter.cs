using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net.Gameplay.Test;

public class BountyHunter : Creature
{
    public BountyHunter(CreatureOutline outline) : base(outline, new CreatureAbilities
    {
        Arcane = true
    })
    {
    }
    #if false

    public override async Task OnPlayAsync()
    {
        RETRY:
        var requireInput = new RequireTargetInputDto(AsDto(), RequireTargetType.Creature, RequireTargetSide.All);
        var output = await PlayerGameState.Nodes
                                          .CardEffectPreview
                                          .RequireTargetsAsync(PlayerGameState.PeerId, requireInput);

        if (output.cardId.HasValue)
        {
            var success = await Resolve(output.cardId.Value);
            if (!success) goto RETRY;
        }
    }


    private async Task<bool> Resolve(Guid cardId)
    {
        if ((PlayerGameState.Board.SingleOrDefault(x => x.Id == cardId) ??
             PlayerGameState.Enemy.Board.SingleOrDefault(x => x.Id == cardId))
            is not CreatureImplementation card) return false;

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());

        card.CreatureState.IsExposed = true;
        await card.UpdateAsync();

        return true;
    }
#endif
}