using System;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class MorneholdSpectre : Creature
{
    public MorneholdSpectre(CreatureOutline outline) :
        base(outline, new CreatureAbilities())
    {
    }
    #if false

    public override async Task OnExitAsync()
    {
        if (!PlayerGameState.Enemy.Board.Any()) return;

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        var board = PlayerGameState.Enemy.Board;

        var idx = Random.Shared.Next(0, board.Count - 1);
        var target = board.ElementAt(idx) as Creature;

        await target!.TakeDamageAsync(3);
        if (target.CreatureState.Def < 1)
        {
            target.MoveToZone(CardZone.None);
            await target.RemoveFromBoardAsync();
            await target.OnExitAsync();
            target.MoveToZone(CardZone.Pit);
        }
    }
#endif
}