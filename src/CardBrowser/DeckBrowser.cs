using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Google.Protobuf;
using OpenCCG.Core;
using OpenCCG.Proto;
using FileAccess = Godot.FileAccess;

namespace OpenCCG.CardBrowser;

public partial class DeckBrowser : ScrollContainer
{
    [Export] private VBoxContainer _container = null!;
    [Export] private TextEdit _deckNameEdit = null!;

    [Export] private CounterLabel
        _creatureCountLabel = null!,
        _spellCountLabel = null!,
        _totalCountLabel = null!;

    [Export] private DeckCountProgressBar
        _cardCountBar0 = null!,
        _cardCountBar1 = null!,
        _cardCountBar2 = null!,
        _cardCountBar3 = null!,
        _cardCountBar4 = null!,
        _cardCountBar5 = null!,
        _cardCountBar6 = null!,
        _cardCountBar7 = null!,
        _cardCountBar8 = null!;

    private static readonly PackedScene CardUiDeckScene = GD.Load<PackedScene>("res://scenes/card-ui-deck.tscn");

    private readonly Dictionary<string, CardUIDeck> _deck = new();

    [Export] private CardStorage _cardStorage = null!;
    [Export] private FileDialog _fileDialog = null!;

    [Export] private Button
        _createDeckButton = null!,
        _saveDeckButton = null!,
        _loadDeckButton = null!;

    public override void _Ready()
    {
        _fileDialog.FileSelected += Load;
        _loadDeckButton.Pressed += () => { _fileDialog.Visible = true; };

        _createDeckButton.Pressed += Clear;

        _saveDeckButton.Pressed += Save;

        ResetCounters();
    }

    public void AddCreature(CreatureOutline creatureOutline)
    {
        if (_deck.TryGetValue(creatureOutline.Id, out var card))
        {
            card.SetCount(card.Count + 1);
            IncreaseCounters(CardType.CreatureType, creatureOutline.Cost);

            return;
        }

        card = CardUiDeckScene.Make<CardUIDeck, CreatureOutline>(creatureOutline, _container);
        var nodes = _container.GetChildren();
        for (var index = 0; index < nodes.Count; index++)
        {
            var child = nodes[index];
            if (child is not CardUIDeck cardUiDeck) continue;
            var cost = cardUiDeck.SpellOutline?.Cost ?? cardUiDeck.CreatureOutline!.Cost;
            if (cost < creatureOutline.Cost) continue;

            _container.MoveChild(card, index);
            break;
        }

        card.GuiInput += inputEvent =>
        {
            if (!inputEvent.IsActionPressed(InputActions.SpriteClick)) return;

            card.SetCount(card.Count - 1);
            DecreaseCounters(CardType.CreatureType, creatureOutline.Cost);
            if (card.Count != 0) return;

            _deck.Remove(creatureOutline.Id);
            card.QueueFree();
        };

        _deck.Add(creatureOutline.Id, card);
        IncreaseCounters(CardType.CreatureType, creatureOutline.Cost);
    }


    public void AddSpell(SpellOutline spellOutline)
    {
        if (_deck.TryGetValue(spellOutline.Id, out var card))
        {
            card.SetCount(card.Count + 1);
            IncreaseCounters(CardType.SpellType, spellOutline.Cost);

            return;
        }

        card = CardUiDeckScene.Make<CardUIDeck, SpellOutline>(spellOutline, _container);
        var nodes = _container.GetChildren();
        for (var index = 0; index < nodes.Count; index++)
        {
            var child = nodes[index];
            if (child is not CardUIDeck cardUiDeck) continue;
            var cost = cardUiDeck.SpellOutline?.Cost ?? cardUiDeck.CreatureOutline!.Cost;
            if (cost < spellOutline.Cost) continue;

            _container.MoveChild(card, index);
            break;
        }

        card.GuiInput += inputEvent =>
        {
            if (!inputEvent.IsActionPressed(InputActions.SpriteClick)) return;

            card.SetCount(card.Count - 1);
            DecreaseCounters(CardType.SpellType, spellOutline.Cost);
            if (card.Count != 0) return;

            _deck.Remove(spellOutline.Id);
            card.QueueFree();
        };

        _deck.Add(spellOutline.Id, card);
        IncreaseCounters(CardType.SpellType, spellOutline.Cost);
    }

    private void Save()
    {
        var deckList = new DeckList
        {
            FileVersion = "1.1",
            Name = _deckNameEdit.Text
        };

        deckList.Entries.AddRange(_deck.Values.Select(x => new DeckListEntry
        {
            CardType = x.CardType,
            Count = x.Count,
            Id = x.Id
        }));
        
        using var file = FileAccess.Open($"user://{_deckNameEdit.Text}.deck", FileAccess.ModeFlags.Write);
        if (file == null) throw new FileLoadException();
        file.StoreBuffer(deckList.ToByteArray());
    }

    private void Load(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) throw new FileLoadException();

        var text = file.GetBuffer((long)file.GetLength());

        var deckList = DeckList.Parser.ParseFrom(text);

        if (deckList == null) throw new FileLoadException();

        Clear();
        _deckNameEdit.Text = deckList.Name;

        foreach (var entry in deckList.Entries)
        {
            switch (entry.CardType)
            {
                case CardType.CreatureType:
                    var creature = _cardStorage.CreaturesById[entry.Id];
                    AddCreature(creature);
                    break;
                case CardType.SpellType:
                    var spell = _cardStorage.SpellsById[entry.Id];
                    AddSpell(spell);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Clear()
    {
        foreach (var child in _container.GetChildren()) child?.QueueFree();

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


    private void IncreaseCounters(CardType cardType, int cost)
    {
        switch (cardType)
        {
            case CardType.CreatureType:
                _creatureCountLabel.Value++;
                break;
            case CardType.SpellType:
                _spellCountLabel.Value++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _totalCountLabel.Value++;
        switch (cost)
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

    private void DecreaseCounters(CardType cardType, int cost)
    {
        switch (cardType)
        {
            case CardType.CreatureType:
                _creatureCountLabel.Value--;
                break;
            case CardType.SpellType:
                _spellCountLabel.Value--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _totalCountLabel.Value--;
        switch (cost)
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
}