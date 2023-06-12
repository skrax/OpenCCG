using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;

namespace OpenCCG;

public partial class EnemyHandArea : Area2D, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardHiddenScene = GD.Load<PackedScene>("res://scenes/card-hidden.tscn");
    private readonly List<Sprite2D> _cards = new();

    private void RemoveCard()
    {
        var cardEntity = _cards.LastOrDefault();
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
        SetCardPositions();
    }

    private void DrawCard()
    {
        var entity = CardHiddenScene.Make<Sprite2D>(this);
        _cards.Add(entity);
        SetCardPositions();
    }

    private void SetCardPositions()
    {
        SpriteHelpers.OrderHorizontally(_cards.ToArray());
    }

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessage(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType) => messageType switch
    {
        MessageType.RemoveCard => IMessageReceiver<MessageType>.MakeExecutor(RemoveCard),
        MessageType.DrawCard => IMessageReceiver<MessageType>.MakeExecutor(DrawCard),
    };
}