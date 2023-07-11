using System.Threading.Tasks;
using OpenCCG.Net;
using Serilog;

namespace OpenCCG.Cards.Test;

public class Equites : CreatureImplementation
{
    public Equites(CreatureOutline outline, PlayerGameState playerGameState) : base(outline, new CreatureAbilities()
    {
        Haste = true
    }, playerGameState)
    {
    }

    public override async Task OnStartCombatAsync()
    {
        var gained = PlayerGameState.Board.Count;
        CreatureState.Atk += gained;
        await UpdateAsync();
    }
}