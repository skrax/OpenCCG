using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Net;
using OpenCCG.Net.ServerNodes;
using Serilog;

namespace OpenCCG.Cards.Test;

public class FieldMedic : CreatureImplementation
{
    public FieldMedic(CreatureOutline outline, PlayerGameState playerGameState) : base(outline, new CreatureAbilities()
    {
        Arcane = true
    }, playerGameState)
    {
    }

    public override async Task OnEnterAsync()
    {
        RETRY:
        var input = new RequireTargetInputDto(AsDto(), RequireTargetType.Creature, RequireTargetSide.All);
        var output = await PlayerGameState.Nodes.CardEffectPreview.RequireTargetsAsync(PlayerGameState.PeerId, input);

        if (output.cardId.HasValue)
        {
            var target = PlayerGameState.Board.SingleOrDefault(x => x.Id == output.cardId) as CreatureImplementation
                         ?? PlayerGameState.Enemy.Board.SingleOrDefault(x => x.Id == output.cardId) as CreatureImplementation;

            if (target is null)
            {
                Log.Warning("Unable to find target for Field Medic Enter {CardId}", output.cardId);
                goto RETRY;
            }

            target.CreatureState.Def += 2;
            await target.UpdateAsync();
        }
    }
}