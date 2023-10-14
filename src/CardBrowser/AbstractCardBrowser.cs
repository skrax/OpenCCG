using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Proto;

namespace OpenCCG.CardBrowser;

public abstract partial class AbstractCardBrowser<TOutline> : ScrollContainer
{
    [Export] protected FlowContainer FlowContainer = null!;
    [Export] protected PackedScene CardUiScene = null!;
    [Export] private DeckBrowser _deckBrowser = null!;

    public override async void _Ready()
    {
        var outlines = await FetchAsync();

        foreach (var cardOutline in outlines)
        {
            var card = CreateCard(cardOutline);

            card.GuiInput += x =>
            {
                if (!x.IsActionPressed(InputActions.SpriteClick)) return;
                
                switch (CardType)
                {
                    case CardType.CreatureType:
                        _deckBrowser.AddCreature((cardOutline as CreatureOutline)!);
                        break;
                    case CardType.SpellType:
                        _deckBrowser.AddSpell((cardOutline as SpellOutline)!);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }
    }

    protected abstract CardType CardType { get; }
    protected abstract Task<TOutline[]> FetchAsync();
    protected abstract CardUI CreateCard(TOutline outline);
}