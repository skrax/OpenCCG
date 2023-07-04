using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Cards.Test;

public class ThrowingKnife : SpellImplementation
{
    public ThrowingKnife(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        var requireInput = new RequireTargetInputDto(AsDto(), RequireTargetType.All, RequireTargetSide.All);
        var output = await PlayerGameState.Nodes
                                          .CardEffectPreview
                                          .RequireTargetsAsync(PlayerGameState.PeerId, requireInput);

        if (output.cardId.HasValue)
        {
            await ResolveCreatureDamage(output.cardId.Value);
        }
        else if (output.isEnemyAvatar.HasValue)
        {
            ResolveAvatarDamage(output.isEnemyAvatar.Value);
        }
    }

    private void ResolveAvatarDamage(bool isEnemyAvatar)
    {
        if (isEnemyAvatar)
        {
            PlayerGameState.Enemy.Health -= 2;
            PlayerGameState.Enemy.UpdateHealth();
        }
        else
        {
            PlayerGameState.Health -= 2;
            PlayerGameState.UpdateHealth();
        }
    }

    private async Task ResolveCreatureDamage(Guid cardId)
    {
        if ((PlayerGameState.Board.SingleOrDefault(x => x.Id == cardId) ??
             PlayerGameState.Enemy.Board.SingleOrDefault(x => x.Id == cardId))
            is not CreatureImplementation card) return;

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        await card.TakeDamageAsync(2);
        if (card.CreatureState.Def <= 0)
        {
            card.MoveToZone(CardZone.None);
            await card.OnExitAsync();
            await card.DestroyAsync();
        }
    }
}