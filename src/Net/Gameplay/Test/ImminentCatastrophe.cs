using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class ImminentCatastrophe : Spell
{
    private const int Damage = 7;

    public ImminentCatastrophe(SpellOutline outline) : base(outline)
    {
    }

    public override void OnPlay()
    {
        throw new System.NotImplementedException();
    }
    #if false

    public override async Task OnPlayAsync()
    {
        var creatures = PlayerGameState.Board
                                       .Concat(PlayerGameState.Enemy.Board)
                                       .Cast<CreatureImplementation>()
                                       .ToList();

        PlayerGameState.Nodes.EnemyCardEffectPreview.TmpShowCard(PlayerGameState.EnemyPeerId, AsDto());

        await Task.WhenAll(creatures.Select(x => x.TakeDamageAsync(Damage)));

        var deadCreatures = creatures.Where(x => x.CreatureState.Def < 1).ToList();

        foreach (var creature in deadCreatures) creature.MoveToZone(CardZone.None);

        await Task.WhenAll(deadCreatures.Select(x => x.RemoveFromBoardAsync()));

        foreach (var creature in deadCreatures)
        {
            await creature.OnExitAsync();
            creature.MoveToZone(CardZone.Pit);
        }
    }
#endif
}