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

    public Dictionary<string, IObserver>? Observers => null;

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
}