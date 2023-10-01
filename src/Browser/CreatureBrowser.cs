using System;
using System.Net.Http.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Proto;
using HttpClient = System.Net.Http.HttpClient;

namespace OpenCCG.Browser;

public partial class CreatureBrowser : ScrollContainer
{
    private readonly HttpClient _httpClient = new();
    [Export] private FlowContainer _flowContainer = null!;
    [Export] private PackedScene _cardUIScene = null!;

    public override async void _Ready()
    {
        _httpClient.BaseAddress = new Uri("https://localhost:7085");
        var creatures = await _httpClient.GetFromJsonAsync<CreatureOutline[]>("Creatures/?orderBy=cost");

        foreach (var cardOutline in creatures)
        {
            AddCardToView(cardOutline);
        }
    }

    private void AddCardToView(CreatureOutline outline)
    {
        var card = _cardUIScene.Make<CardUI, CreatureOutline>(outline, _flowContainer);
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