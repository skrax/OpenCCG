using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Cards.Test;

public class ThrowingKnife : SpellImplementation
{
    private const int Damage = 2;
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

    private async Task ResolveCreatureDamage(Guid cardId)
    {
        if ((PlayerGameState.Board.SingleOrDefault(x => x.Id == cardId) ??
             PlayerGameState.Enemy.Board.SingleOrDefault(x => x.Id == cardId))
            is not CreatureImplementation card) return;

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        await card.TakeDamageAsync(Damage);
        if (card.CreatureState.Def <= 0)
        {
            card.MoveToZone(CardZone.None);
            await card.RemoveFromBoardAsync();
            await card.OnExitAsync();
            card.MoveToZone(CardZone.Pit);
        }
    }
}