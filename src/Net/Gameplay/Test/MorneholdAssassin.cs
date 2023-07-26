using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net.Gameplay.Test;

public class MorneholdAssassin : Creature
{
    private const int Damage = 2;

    public MorneholdAssassin(CreatureOutline outline) :
        base(outline, new CreatureAbilities())
    {
    }
    
    #if false

    public override async Task OnPlayAsync()
    {
        RETRY:
        var requireInput = new RequireTargetInputDto(AsDto(), RequireTargetType.All, RequireTargetSide.All);
        var output = await PlayerGameState.Nodes
                                          .CardEffectPreview
                                          .RequireTargetsAsync(PlayerGameState.PeerId, requireInput);

        if (output.cardId.HasValue)
        {
            var success = await ResolveCreatureDamage(output.cardId.Value);
            if (!success) goto RETRY;
        }
        else if (output.isEnemyAvatar.HasValue)
        {
            ResolveAvatarDamage(output.isEnemyAvatar.Value);
        }
    }

    private void ResolveAvatarDamage(bool isEnemyAvatar)
    {
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        if (isEnemyAvatar)
        {
            PlayerGameState.Enemy.Health -= Damage;
            PlayerGameState.Enemy.UpdateHealth();
        }
        else
        {
            PlayerGameState.Health -= Damage;
            PlayerGameState.UpdateHealth();
        }
    }

    private async Task<bool> ResolveCreatureDamage(Guid cardId)
    {
        if ((PlayerGameState.Board.SingleOrDefault(x => x.Id == cardId) ??
             PlayerGameState.Enemy.Board.SingleOrDefault(x => x.Id == cardId))
            is not CreatureImplementation card) return false;

        if (!Abilities.Arcane && !card.CreatureState.IsExposed) return false;

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        await card.TakeDamageAsync(Damage);
        if (card.CreatureState.Def <= 0)
        {
            card.MoveToZone(CardZone.None);
            await card.RemoveFromBoardAsync();
            await card.OnExitAsync();
            card.MoveToZone(CardZone.Pit);
        }

        return true;
    }
#endif
}