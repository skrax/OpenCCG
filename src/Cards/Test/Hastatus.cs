using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class Hastatus : CreatureImplementation
{
    public Hastatus(CreatureOutline outline, PlayerGameState playerGameState) : base(outline, new CreatureAbilities(),
        playerGameState)
    {
    }

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
}