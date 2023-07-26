using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class CallToArms : Spell
{
    public CallToArms(SpellOutline outline) : base(outline)
    {
    }

    public override void OnPlay()
    {
    }
    
#if false
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
#endif
}