using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Proto;

namespace OpenCCG.CardBrowser;

public partial class CreatureBrowser : AbstractCardBrowser<CreatureOutline>
{
    [Export] private CardStorage _cardStorage = null!;

    protected override CardType CardType => CardType.CreatureType;

    protected override async Task<CreatureOutline[]> FetchAsync()
    {
        await _cardStorage.DataReady;

        return _cardStorage.Creatures;
    }

    protected override CardUI CreateCard(CreatureOutline outline)
    {
        return CardUiScene.Make<CardUI, CreatureOutline>(outline, FlowContainer);
    }
}