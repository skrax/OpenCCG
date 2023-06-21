using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using FileAccess = Godot.FileAccess;

namespace OpenCCG;

public partial class CardBrowser : Control
{
    private static readonly PackedScene CardUIScene = GD.Load<PackedScene>("res://scenes/card-ui.tscn");
    private static readonly PackedScene CardUIDeckScene = GD.Load<PackedScene>("res://scenes/card-ui-deck.tscn");
    private static readonly PackedScene MenuScene = GD.Load<PackedScene>("res://scenes/menu.tscn");

    private readonly Dictionary<string, CardUIDeck> _deck = new();
    [Export] private HBoxContainer _bottomPanelContainer;
    [Export] private FlowContainer _cardViewFlowContainer;

    [Export] private ScrollContainer _cardViewScrollContainer, _deckScrollContainer;
    [Export] private Button _clearTextButton;
    [Export] private VBoxContainer _deckContainer;
    [Export] private FileDialog _fileDialog;
    [Export] private Button _menuButton, _createDeckButton, _saveDeckButton, _loadDeckButton;
    [Export] private TextEdit _searchEdit, _deckNameEdit;

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

    [Export] private CounterLabel _creatureCountLabel, _spellCountLabel, _totalCountLabel;

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

            foreach (var cardRecord in Database.Cards.Values) AddCardToView(cardRecord);
        };

        _searchEdit.TextChanged += () =>
        {
            var text = _searchEdit.Text;
            foreach (var child in _cardViewFlowContainer.GetChildren())
                if (!child.IsQueuedForDeletion())
                    child.QueueFree();

            foreach (var cardRecord in Database.Cards.Values.Where(x => x.Description.Contains(text)))
                AddCardToView(cardRecord);
        };

        foreach (var cardRecord in Database.Cards.Values.OrderBy(x => x.Cost)) AddCardToView(cardRecord);

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


    private void AddCardToView(CardRecord cardRecord)
    {
        var card = CardUIScene.Make<CardUI, CardRecord>(cardRecord, _cardViewFlowContainer);
        card.GuiInput += x =>
        {
            if (!x.IsActionPressed(InputActions.SpriteClick)) return;

            if (_deck.TryGetValue(cardRecord.Id, out var cardDeck))
            {
                cardDeck.SetCount(cardDeck.Count + 1);
                IncreaseCounters(cardDeck.Record);
            }
            else
            {
                AddCardToDeck(card.Record);

                IncreaseCounters(cardRecord);
            }
        };
    }

    private CardUIDeck AddCardToDeck(CardRecord cardRecord)
    {
        var cardDeck = CardUIDeckScene.Make<CardUIDeck, CardRecord>(cardRecord, _deckContainer);
        var nodes = _deckContainer.GetChildren();
        for (var index = 0; index < nodes.Count; index++)
        {
            var child = nodes[index];
            if (child is not CardUIDeck cardUiDeck) continue;
            if (cardUiDeck.Record.Cost < cardRecord.Cost) continue;

            _deckContainer.MoveChild(cardDeck, index);
            break;
        }

        cardDeck.GuiInput += inputEvent =>
        {
            if (!inputEvent.IsActionPressed(InputActions.SpriteClick)) return;

            cardDeck.SetCount(cardDeck.Count - 1);
            DecreaseCounters(cardDeck.Record);
            if (cardDeck.Count != 0) return;

            _deck.Remove(cardRecord.Id);
            cardDeck.QueueFree();
        };
        _deck.Add(cardRecord.Id, cardDeck);

        return cardDeck;
    }

    private void IncreaseCounters(CardRecord cardRecord)
    {
        switch (cardRecord.Type)
        {
            case CardRecordType.Creature:
                _creatureCountLabel.Value++;
                break;
            case CardRecordType.Spell:
                _spellCountLabel.Value++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _totalCountLabel.Value++;
        switch (cardRecord.Cost)
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
    
    private void DecreaseCounters(CardRecord cardRecord)
    {
        switch (cardRecord.Type)
        {
            case CardRecordType.Creature:
                _creatureCountLabel.Value--;
                break;
            case CardRecordType.Spell:
                _spellCountLabel.Value--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _totalCountLabel.Value--;
        switch (cardRecord.Cost)
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
        var json = JsonSerializer.Serialize(_deck.Values.Select(x => x.ToJsonRecord()).ToList());

        using var file = FileAccess.Open($"user://{_deckNameEdit.Text}.deck", FileAccess.ModeFlags.Write);
        if (file == null) throw new FileLoadException();
        file.StoreString(json);
    }


    private void LoadDeck(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) throw new FileLoadException();


        var deserialized = JsonSerializer.Deserialize<CardUIDeck.JsonRecord[]>(file.GetAsText());

        if (deserialized == null) throw new FileLoadException();

        ClearDeck();
        _deckNameEdit.Text = Path.GetFileNameWithoutExtension(path);

        foreach (var jsonRecord in deserialized)
        {
            var cardRecord = Database.Cards[jsonRecord.Id];
            AddCardToDeck(cardRecord).SetCount(jsonRecord.Count);
            for (var i = 0; i < jsonRecord.Count; ++i) IncreaseCounters(cardRecord);
        }
    }
}