using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Cards.Test;
using OpenCCG.Core;
using FileAccess = Godot.FileAccess;

namespace OpenCCG;

public partial class CardBrowser : Control
{
    private static readonly PackedScene CardUIScene = GD.Load<PackedScene>("res://scenes/card-ui.tscn");
    private static readonly PackedScene CardUIDeckScene = GD.Load<PackedScene>("res://scenes/card-ui-deck.tscn");
    private static readonly PackedScene MenuScene = GD.Load<PackedScene>("res://scenes/menu.tscn");

    private readonly Dictionary<string, CardUIDeck> _deck = new();
    [Export] private HBoxContainer _bottomPanelContainer;

    [Export] private DeckCountProgressBar
        _cardCountBar0,
        _cardCountBar1,
        _cardCountBar2,
        _cardCountBar3,
        _cardCountBar4,
        _cardCountBar5,
        _cardCountBar6,
        _cardCountBar7,
        _cardCountBar8;

    [Export] private FlowContainer _cardViewFlowContainer;

    [Export] private ScrollContainer _cardViewScrollContainer, _deckScrollContainer;
    [Export] private Button _clearTextButton;

    [Export] private CounterLabel _creatureCountLabel, _spellCountLabel, _totalCountLabel;
    [Export] private VBoxContainer _deckContainer;
    [Export] private FileDialog _fileDialog;
    [Export] private Button _menuButton, _createDeckButton, _saveDeckButton, _loadDeckButton;
    [Export] private TextEdit _searchEdit, _deckNameEdit;

    public override void _Ready()
    {
        _menuButton.Pressed += () => { GetTree().ChangeSceneToPacked(MenuScene); };

        _fileDialog.FileSelected += LoadDeck;
        _loadDeckButton.Pressed += () => { _fileDialog.Visible = true; };

        _createDeckButton.Pressed += ClearDeck;

        _saveDeckButton.Pressed += SaveDeck;

        _clearTextButton.Pressed += () =>
        {
            Logger.Info("text cleared");
            _searchEdit.Text = "";

            foreach (var child in _cardViewFlowContainer.GetChildren())
                if (!child.IsQueuedForDeletion())
                    child.QueueFree();

            foreach (var outline in TestSetOutlines.All) AddCardToView(outline);
        };

        _searchEdit.TextChanged += () =>
        {
            var text = _searchEdit.Text;
            foreach (var child in _cardViewFlowContainer.GetChildren())
                if (!child.IsQueuedForDeletion())
                    child.QueueFree();

            foreach (var outline in TestSetOutlines.All.Where(x => x.Description.Contains(text)))
            {
                AddCardToView(outline);
            }
        };

        foreach (var outline in TestSetOutlines.All.OrderBy(x => x.Cost))
        {
            AddCardToView(outline);
        }

        ResetCounters();
    }

    private void ClearDeck()
    {
        foreach (var child in _deckContainer.GetChildren()) child?.QueueFree();

        _deck.Clear();
        _deckNameEdit.Clear();

        ResetCounters();
    }

    private void ResetCounters()
    {
        _creatureCountLabel.Value = 0;
        _spellCountLabel.Value = 0;
        _totalCountLabel.Value = 0;
        _cardCountBar0.Count = 0;
        _cardCountBar1.Count = 0;
        _cardCountBar2.Count = 0;
        _cardCountBar3.Count = 0;
        _cardCountBar4.Count = 0;
        _cardCountBar5.Count = 0;
        _cardCountBar6.Count = 0;
        _cardCountBar7.Count = 0;
        _cardCountBar8.Count = 0;
    }


    private void AddCardToView(ICardOutline outline)
    {
        var card = CardUIScene.Make<CardUI, ICardOutline>(outline, _cardViewFlowContainer);
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
    }

    private CardUIDeck AddCardToDeck(ICardOutline outline)
    {
        var cardDeck = CardUIDeckScene.Make<CardUIDeck, ICardOutline>(outline, _deckContainer);
        var nodes = _deckContainer.GetChildren();
        for (var index = 0; index < nodes.Count; index++)
        {
            var child = nodes[index];
            if (child is not CardUIDeck cardUiDeck) continue;
            if (cardUiDeck.Outline.Cost < outline.Cost) continue;

            _deckContainer.MoveChild(cardDeck, index);
            break;
        }

        cardDeck.GuiInput += inputEvent =>
        {
            if (!inputEvent.IsActionPressed(InputActions.SpriteClick)) return;

            cardDeck.SetCount(cardDeck.Count - 1);
            DecreaseCounters(cardDeck.Outline);
            if (cardDeck.Count != 0) return;

            _deck.Remove(outline.Id);
            cardDeck.QueueFree();
        };
        _deck.Add(outline.Id, cardDeck);

        return cardDeck;
    }

    private void IncreaseCounters(ICardOutline outline)
    {
        switch (outline)
        {
            case ICreatureOutline:
                _creatureCountLabel.Value++;
                break;
            case ISpellOutline:
                _spellCountLabel.Value++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _totalCountLabel.Value++;
        switch (outline.Cost)
        {
            case 0:
                _cardCountBar0.Count++;
                break;
            case 1:
                _cardCountBar1.Count++;
                break;
            case 2:
                _cardCountBar2.Count++;
                break;
            case 3:
                _cardCountBar3.Count++;
                break;
            case 4:
                _cardCountBar4.Count++;
                break;
            case 5:
                _cardCountBar5.Count++;
                break;
            case 6:
                _cardCountBar6.Count++;
                break;
            case 7:
                _cardCountBar7.Count++;
                break;
            case >= 8:
                _cardCountBar8.Count++;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void DecreaseCounters(ICardOutline outline)
    {
        switch (outline)
        {
            case ICreatureOutline:
                _creatureCountLabel.Value--;
                break;
            case ISpellOutline:
                _spellCountLabel.Value--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _totalCountLabel.Value--;
        switch (outline.Cost)
        {
            case 0:
                _cardCountBar0.Count--;
                break;
            case 1:
                _cardCountBar1.Count--;
                break;
            case 2:
                _cardCountBar2.Count--;
                break;
            case 3:
                _cardCountBar3.Count--;
                break;
            case 4:
                _cardCountBar4.Count--;
                break;
            case 5:
                _cardCountBar5.Count--;
                break;
            case 6:
                _cardCountBar6.Count--;
                break;
            case 7:
                _cardCountBar7.Count--;
                break;
            case >= 8:
                _cardCountBar8.Count--;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void SaveDeck()
    {
        var deck = new SavedDeck("1.0", _deck.Values.Select(x => x.ToJsonRecord()).ToArray());
        var json = JsonSerializer.Serialize(deck);

        using var file = FileAccess.Open($"user://{_deckNameEdit.Text}.deck", FileAccess.ModeFlags.Write);
        if (file == null) throw new FileLoadException();
        file.StoreString(json);
    }


    private void LoadDeck(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) throw new FileLoadException();


        var deserialized = JsonSerializer.Deserialize<SavedDeck>(file.GetAsText());

        if (deserialized == null) throw new FileLoadException();
        if (deserialized.format != "1.0") throw new FileLoadException($"Unsupported Deck Format {deserialized.format}");

        ClearDeck();
        _deckNameEdit.Text = Path.GetFileNameWithoutExtension(path);

        foreach (var jsonRecord in deserialized.list)
        {
            var outline = TestSetOutlines.Cards[jsonRecord.Id];
            AddCardToDeck(outline).SetCount(jsonRecord.Count);
            for (var i = 0; i < jsonRecord.Count; ++i)
            {
                IncreaseCounters(outline);
            }
        }
    }

    private record SavedDeck(string format, CardUIDeck.JsonRecord[] list);
}