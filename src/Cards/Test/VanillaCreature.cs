using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class VanillaCreature : CreatureImplementation
{
    public VanillaCreature(ICreatureOutline outline, CreatureAbilities abilities, PlayerGameState2 playerGameState
    ) : base(
        outline, abilities, playerGameState)
    {
    }
}