using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class HandArea : Area2D, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardScene = GD.Load<PackedScene>("res://scenes/card.tscn");
    private readonly List<Card> _cards = new();

    private void RemoveCard(Guid cardId)
    {
        var cardEntity = _cards.FirstOrDefault(x => x.Id == cardId);
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
        SetCardPositions();
    }

    private void FailPlayCard()
    {
        SetCardPositions();
    }

    private void DrawCard(CardGameStateDto card)
    {
        var entity = CardScene.Make<Card, CardGameStateDto>(card, this);
        entity.OnDragFailed += SetCardPositions;

        _cards.Add(entity);
        SetCardPositions();
    }

    private void SetCardPositions()
    {
        SpriteHelpers.OrderHorizontally(_cards.Cast<Sprite2D>().ToArray());
    }

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public void HandleMessage(string messageJson)
    {
        IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
    }

    public Func<int, string?, string?> GetExecutor(MessageType messageType) => messageType switch
    {
        MessageType.RemoveCard => IMessageReceiver<MessageType>.MakeExecutor<Guid>(RemoveCard),
        MessageType.FailPlayCard => IMessageReceiver<MessageType>.MakeExecutor(FailPlayCard),
        MessageType.DrawCard => IMessageReceiver<MessageType>.MakeExecutor<CardGameStateDto>(DrawCard)
    };
}