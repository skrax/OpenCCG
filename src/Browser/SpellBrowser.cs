using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Proto;

namespace OpenCCG.Browser;

public partial class SpellBrowser : AbstractCardBrowser<SpellOutline>
{
    [Export] private CardStorage _cardStorage = null!;

    protected override CardType CardType => CardType.SpellType;

    protected override async Task<SpellOutline[]> FetchAsync()
    {
        await _cardStorage.DataReady;

        return _cardStorage.Spells;
    }

    protected override CardUI CreateCard(SpellOutline outline)
    {
        return CardUiScene.Make<CardUI, SpellOutline>(outline, FlowContainer);
    }
}