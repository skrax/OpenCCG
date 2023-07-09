using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class DeepAnalysis : SpellImplementation
{
    private const int Amount = 2;

    public DeepAnalysis(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        await PlayerGameState.DrawAsync(Amount);
    }
}