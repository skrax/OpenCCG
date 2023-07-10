using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class HeedTheCall : SpellImplementation
{
    public HeedTheCall(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());

        for (var i = 0; i < 2; ++i)
        {
            var spectre =
                (CreatureImplementation)TestSetImplementations.GetImplementation("TEST-C-008", PlayerGameState);
            spectre.MoveToZone(CardZone.Board);
            await spectre.EnterBoardAsync();
        }
    }
}