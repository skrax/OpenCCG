using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Api;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class HandArea : Area2D, IHandRpc
{
    private static readonly PackedScene CardScene = GD.Load<PackedScene>("res://scenes/card.tscn");
    private readonly List<Card> _cards = new();

    [Rpc]
    public void RemoveCard(string id)
    {
        var cardId = Guid.Parse(id);
        var cardEntity = _cards.FirstOrDefault(x => x.Id == cardId);
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
        SetCardPositions();
    }

    [Rpc]
    public void FailPlayCard()
    {
        SetCardPositions();
    }

    [Rpc]
    public void DrawCard(string cardGameStateJson)
    {
        var card = JsonSerializer.Deserialize<CardGameStateDto>(cardGameStateJson);
        var entity = CardScene.Make<Card, CardGameStateDto>(card, this);
        entity.OnDragFailed += SetCardPositions;

        _cards.Add(entity);
        SetCardPositions();
    }

    private void SetCardPositions()
    {
        SpriteHelpers.OrderHorizontally(_cards.Cast<Sprite2D>().ToArray());
    }
}