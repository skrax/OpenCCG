using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class VanillaCreature : CreatureImplementation
{
    public VanillaCreature(CreatureOutline outline, CreatureAbilities abilities, PlayerGameState playerGameState
    ) : base(
        outline, abilities, playerGameState)
    {
    }
}