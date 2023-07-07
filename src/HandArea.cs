using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG;

public partial class HandArea : HBoxContainer, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardScene = GD.Load<PackedScene>("res://scenes/card.tscn");
    private readonly List<Card> _cards = new();
    [Export] private Curve _heightCurve, _rotationCurve, _separationCurve;

    public Dictionary<string, IObserver>? Observers => null;

    public override void _Ready()
    {
        SortChildren += CustomSort;
        PreSortChildren += PreCustomSort;
    }

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.RemoveCard => Executor.Make<Guid>(RemoveCard, Executor.ResponseMode.NoResponse),
            MessageType.DrawCard => Executor.Make<CardImplementationDto>(DrawCard, Executor.ResponseMode.Respond)
        };
    }

    private void RemoveCard(Guid cardId)
    {
        var cardEntity = _cards.FirstOrDefault(x => x.Id == cardId);
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
    }

    private void DrawCard(CardImplementationDto card)
    {
        var entity = CardScene.Make<Card, CardImplementationDto>(card, this);
        _cards.Add(entity);
    }

    private void PreCustomSort()
    {
        if (!_cards.Any()) return;
        var separation = (int)_separationCurve.Sample((_cards.Count - 0f) / 12f) * -40;

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
                Logger.Info<HandArea>($"{index}/{_cards.Count} -> {sampleIndex}");
                var pos = c.GlobalPosition;
                pos.Y -= _heightCurve.Sample(sampleIndex) * 18;
                c.GlobalPosition = pos;
                c.RotationDegrees = _rotationCurve.Sample(sampleIndex) * 4f;
            }

            c.ZIndex = _cards.Count - index;
        }
    }
}