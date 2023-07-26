using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class Hastatus : Creature
{
    public Hastatus(CreatureOutline outline) : base(outline, new CreatureAbilities())
    {
    }
    #if false

    public override async Task OnPlayAsync()
    {
        var copy = Copy();
        copy.MoveToZone(CardZone.Board);
        await copy.EnterBoardAsync();
        await copy.OnEnterAsync();
    }

    private Hastatus Copy()
    {
        var copy = new Hastatus(CreatureOutline, PlayerGameState);
        copy.Abilities = Abilities.Copy();
        copy.State = CreatureState.Copy();

        return copy;
    }
#endif
}