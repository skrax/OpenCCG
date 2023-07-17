using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class ChampionOfNargaeya : CreatureImplementation
{
    public ChampionOfNargaeya(CreatureOutline outline, PlayerGameState playerGameState) 
        : base(outline, new CreatureAbilities(), playerGameState)
    {
    }

    public override async Task OnEndCombatAsync()
    {
        CreatureState.Atk += 1;
        CreatureState.Def += 1;
        await UpdateAsync();
    }
}