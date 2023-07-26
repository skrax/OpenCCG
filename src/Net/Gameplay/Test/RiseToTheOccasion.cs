using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class RiseToTheOccasion : Spell
{
    public RiseToTheOccasion(SpellOutline outline) : base(outline)
    {
    }

    public override void OnPlay()
    {
        throw new System.NotImplementedException();
    }

#if false

    public override async Task OnPlayAsync()
    {
        if (PlayerGameState.Deck.FirstOrDefault(x => x is Creature)
            is not Creature creature) return;

        creature.CreatureState.Atk += 5;
        creature.CreatureState.Def += 5;
        creature.Abilities.Haste = true;
        creature.CreatureState.AddedText = "[center][b]\n*Haste*\n[/b][/center]";

        await creature.UpdateAsync();
        
        PlayerGameState.Deck.Remove(creature);
        PlayerGameState.Deck.AddFirst(creature);

        await PlayerGameState.DrawAsync();
    }
#endif
}