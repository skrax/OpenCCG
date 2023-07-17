using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class CallToArms : SpellImplementation
{
    public CallToArms(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        var tasks = new List<Task>();
        foreach (var creature in PlayerGameState.Board.Cast<CreatureImplementation>())
        {
            creature.CreatureState.Atk += 2;
            creature.CreatureState.Def += 2;
            tasks.Add(creature.UpdateAsync());
        }

        await Task.WhenAll(tasks);
    }
}