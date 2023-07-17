using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class RiseToTheOccasion : SpellImplementation
{
    public RiseToTheOccasion(SpellOutline outline, PlayerGameState playerGameState) : base(outline, playerGameState)
    {
    }

    public override async Task OnPlayAsync()
    {
        if (PlayerGameState.Deck.FirstOrDefault(x => x is CreatureImplementation)
            is not CreatureImplementation creature) return;

        creature.CreatureState.Atk += 5;
        creature.CreatureState.Def += 5;
        creature.Abilities.Haste = true;
        creature.CreatureState.AddedText = "[center][b]\n*Haste*\n[/b][/center]";

        await creature.UpdateAsync();
        
        PlayerGameState.Deck.Remove(creature);
        PlayerGameState.Deck.AddFirst(creature);

        await PlayerGameState.DrawAsync();
    }
}