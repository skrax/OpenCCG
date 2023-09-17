using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Net.Messaging;

namespace OpenCCG.GameBoard;

[GlobalClass]
public partial class Hand : HBoxContainer
{
    [Export] private PackedScene _cardScene = null!;
    [Export] private Curve _heightCurve = null!, _rotationCurve = null!, _separationCurve = null!;
    [Export] private AnimationQueue _animationQueue = null!;
    [Export] private MessageBroker _broker = null!;
    private readonly List<Card> _cards = new();
    private readonly HashSet<Guid> _addedCardIds = new();

    public override void _Ready()
    {
        SortChildren += CustomSort;
        PreSortChildren += CustomPreSort;
    }

    public void RemoveCard(Guid cardId)
    {
        var cardEntity = _cards.FirstOrDefault(x => x.Id == cardId);
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
    }

    public void AddCard(CardImplementationDto card)
    {
        var entity = _cardScene.Instantiate<Card>();
        entity.Init(card, _broker);
        AddChild(entity);
        _cards.Add(entity);
        _addedCardIds.Add(entity.Id);
    }

    private void CustomPreSort()
    {
        if (!_cards.Any()) return;
        var separation = (int)_separationCurve.Sample((_cards.Count - 0f) / 12f) * -20;

        if (HasThemeConstantOverride("separation"))
            AddThemeConstantOverride("separation", separation);
    }

    private void CustomSort()
    {
        for (var index = 0; index < _cards.Count; index++)
        {
            var c = _cards[index];

            if (_cards.Count > 3)
            {
                var sampleIndex = (float)(index - 0) / (_cards.Count - 1 - 0);
                var pos = c.GlobalPosition;
                pos.Y -= _heightCurve.Sample(sampleIndex) * 18;
                c.GlobalPosition = pos;
                c.RotationDegrees = _rotationCurve.Sample(sampleIndex) * 4f;
            }

            if (_addedCardIds.Remove(c.Id))
            {
                _animationQueue.Enqueue(async () =>
                    await c.PlayDrawAnimAsync(new Vector2(1921, c.GlobalPosition.Y), c.GlobalPosition));
            }

            c.ZIndex = _cards.Count - index;
        }
    }
}