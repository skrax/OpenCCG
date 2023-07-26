using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class HeedTheCall : Spell
{
    public HeedTheCall(SpellOutline outline) : base(outline)
    {
    }

    public override void OnPlay()
    {
        throw new System.NotImplementedException();
    }
    #if false

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
#endif
}