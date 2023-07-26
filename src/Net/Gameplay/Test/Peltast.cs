using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class Peltast : Creature
{
    public Peltast(CreatureOutline outline) : base(outline, new CreatureAbilities() )
    {
    }
    #if false

    public override Task OnStartCombatAsync()
    {
        PlayerGameState.Enemy.Health -= 2;
        PlayerGameState.Enemy.UpdateHealth();
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        return Task.CompletedTask;
    }
#endif
}