using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class DeepAnalysis : SpellImplementation
{
    private const int Amount = 2;

    public DeepAnalysis(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override Task OnPlayAsync()
    {
        PlayerGameState.Draw(Amount);
        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());
        return Task.CompletedTask;
    }
}