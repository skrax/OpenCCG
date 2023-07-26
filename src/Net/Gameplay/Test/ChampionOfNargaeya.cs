using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class ChampionOfNargaeya : Creature
{
    public ChampionOfNargaeya(CreatureOutline outline) 
        : base(outline, new CreatureAbilities())
    {
    }
#if false
    public override async Task OnEndCombatAsync()
    {
        CreatureState.Atk += 1;
        CreatureState.Def += 1;
        await UpdateAsync();
    }
#endif
}