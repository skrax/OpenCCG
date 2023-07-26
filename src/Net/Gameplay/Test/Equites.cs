using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class Equites : Creature
{
    public Equites(CreatureOutline outline) : base(outline, new CreatureAbilities()
    {
        Haste = true
    })
    {
    }
#if false
    public override async Task OnStartCombatAsync()
    {
        var gained = PlayerGameState.Board.Count;
        CreatureState.Atk += gained;
        await UpdateAsync();
    }
#endif
}