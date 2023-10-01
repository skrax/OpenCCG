using System;
using System.Net.Http.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Proto;
using HttpClient = System.Net.Http.HttpClient;

namespace OpenCCG.Browser;

public partial class SpellBrowser : ScrollContainer
{
    private readonly HttpClient _httpClient = new();
    [Export] private FlowContainer _flowContainer = null!;
    [Export] private PackedScene _cardUIScene = null!;

    public override async void _Ready()
    {
        _httpClient.BaseAddress = new Uri("https://localhost:7085");
        var spells = await _httpClient.GetFromJsonAsync<SpellOutline[]>("Spells?orderBy=cost");

        foreach (var cardOutline in spells)
        {
            AddCardToView(cardOutline);
        }
    }

    private void AddCardToView(SpellOutline outline)
    {
        var card = _cardUIScene.Make<CardUI, SpellOutline>(outline, _flowContainer);
#if false
            card.GuiInput += x =>
            {
                if (!x.IsActionPressed(InputActions.SpriteClick)) return;
    
                if (_deck.TryGetValue(outline.Id, out var cardDeck))
                {
                    cardDeck.SetCount(cardDeck.Count + 1);
                    IncreaseCounters(cardDeck.Outline);
                }
                else
                {
                    AddCardToDeck(card.Outline);
    
                    IncreaseCounters(outline);
                }
            };
#endif
    }
}