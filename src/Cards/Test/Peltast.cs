using System.Threading.Tasks;
using OpenCCG.Net;
using Serilog;

namespace OpenCCG.Cards.Test;

public class Peltast : CreatureImplementation
{
    public Peltast(CreatureOutline outline, PlayerGameState playerGameState) : base(outline, new CreatureAbilities(),
        playerGameState)
    {
    }

    public override Task OnStartCombatAsync()
    {
        PlayerGameState.Enemy.Health -= 2;
        PlayerGameState.Enemy.UpdateHealth();
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        return Task.CompletedTask;
    }
}