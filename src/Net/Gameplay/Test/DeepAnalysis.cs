using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class DeepAnalysis : Spell
{
    private const int Amount = 2;

    public DeepAnalysis(SpellOutline outline) : base(outline)
    {
    }

    public override void OnPlay()
    {
    }
#if false
    public override async Task OnPlayAsync()
    {
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        await PlayerGameState.DrawAsync(Amount);
    }
#endif
}